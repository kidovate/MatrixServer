using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MatrixAPI.Data;
using MatrixAPI.Encryption;
using MatrixAPI.Enum;
using MatrixHost.Nodes;
using ProtoBuf;
using ZeroMQ;
using log4net;

namespace MatrixHost.MasterInterface
{
    /// <summary>
    /// A connection to a MasterServer. Manages the host.
    /// </summary>
    public class HostClient
    {
        private ZmqContext context;
        private ZmqSocket socket;
        private ZmqSocket finalSocket;

        private string masterIp;
        private int masterPort;

        private Thread socketThread;

        private byte[] assignedIdentity;

        private int status = 0;

        private static readonly ILog log = LogManager.GetLogger(typeof(HostClient));

        private System.Timers.Timer heartbeat = new System.Timers.Timer(3000);

        private AES encryption;

        private byte[] keyHash;

        private Dictionary<int, bool> NodeExistsResponses = new Dictionary<int, bool>();

        private Dictionary<int, NodeInfo> NodeDictionary = new Dictionary<int, NodeInfo>(); 

        public static HostClient Instance;


        private Dictionary<int, string> receivedNodeURLs = new Dictionary<int, string>();

        private int heartbeatAttempts = 0;

        /// <summary>
        /// Create a new host client.
        /// </summary>
        /// <param name="serverIp">Master server IP</param>
        /// <param name="serverPort">Master server port.</param>
        public HostClient(string serverIp, int serverPort, AES encryption, byte[] keyHash)
        {
            log.Info("Creating a new HostClient.");
            heartbeat.Elapsed += (sender, args) => SendHeartbeat();
            heartbeat.Stop();
            Instance = this;
            this.encryption = encryption;
            this.keyHash = keyHash;
            context = ZmqContext.Create();
            socket = context.CreateSocket(SocketType.DEALER);
            socket.TcpKeepalive = TcpKeepaliveBehaviour.Enable;
            masterIp = serverIp;
            masterPort = serverPort;
        }

        /// <summary>
        /// Conditionally send out a heartbeat
        /// </summary>
        private void SendHeartbeat()
        {
            if(heartbeatAttempts > 4)
            {
                log.Debug("Heartbeat timeout for "+heartbeat.Interval*heartbeatAttempts/1000+" seconds...");
                heartbeatAttempts++;
                return;
            }

            socket.Send(BuildMessage(MessageIdentifier.Heartbeat, null, true));
            heartbeatAttempts++;
        }

        public void Startup()
        {
            log.Info("HostClient starting up...");
            if (status == 1) return;

            socketThread = new Thread(ClientThread);
            socketThread.Start();
        }

        private void ClientThread()
        {
            status = 1;
            log.Debug("Connecting to server "+masterIp+":"+masterPort);
            bool connected = false;
            {
                byte[] identity = new byte[15];
                new Random().NextBytes(identity);
                socket.Identity = identity;
                var helloMessage = new[]{(byte)MessageIdentifier.Init};
                int attempt = 1;
                var endpoint = "tcp://" + masterIp + ":" + masterPort;
                socket.Connect(endpoint);
                while (!connected)
                {
                    
                    socket.Send(helloMessage);
                    var message = socket.ReceiveMessage(TimeSpan.FromSeconds(5));
                    if(message.TotalSize == 0)
                    {
                        log.Error("("+attempt+") Failed to connect, trying again in 5 seconds...");
                    }else
                    {
                        log.Debug("Received response, verifying...");

                        //check command value
                        if(message.First.Buffer[0] == (byte)MessageIdentifier.SetIdentity)
                        {
                            assignedIdentity = message.First.Buffer.Skip(1).ToArray();
                            log.Debug("Received a new "+assignedIdentity.Length+" byte identity.");
                            socket.Identity = assignedIdentity;
                            socket.Disconnect(endpoint);
                            finalSocket = context.CreateSocket(SocketType.DEALER);
                            socket.Dispose();
                            socket = finalSocket;
                            socket.Identity = assignedIdentity;
                            socket.TcpKeepalive = TcpKeepaliveBehaviour.Enable;
                            socket.Connect(endpoint);
                            socket.Send(new byte[] {(byte) MessageIdentifier.SetIdentity});
                            break;
                        }
                        if(message.First.Buffer[0] == (byte)MessageIdentifier.InvalidIdentity)
                        {
                            log.Error("Server responded with invalid identity. Trying again.");
                        }
                    }
                    attempt++;
                    Thread.Sleep(5000);
                }
                log.Info("Connected to master server after "+attempt+" attempts.");
            }
            //We are connected!
            log.Debug("Waiting for encryption sequence...");
            var msg = socket.ReceiveMessage();
            if (msg.First.Buffer[0] != (byte)MessageIdentifier.BeginEncryption)
            {
                LogUnexpectedMessage(msg.First.Buffer);
                log.Error("Crucial unexpected result, disconnecting.");
                socket.Send(new byte[] {(byte) MessageIdentifier.Disconnect});
                status = 0;
                return;
            }

            log.Debug("Sending encryption key...");
            byte[] encryptMsg = new byte[1+keyHash.Length];
            encryptMsg[0] = (byte) MessageIdentifier.BeginEncryption;
            keyHash.CopyTo(encryptMsg, 1);
            socket.Send(encryptMsg);

            msg = socket.ReceiveMessage();

            if(msg.First.Buffer[0] != (byte)MessageIdentifier.ConfirmEncryption)
            {
                log.Error("Invalid encryption key. Exiting...");
                status = 0;
                socket.Send(BuildMessage(MessageIdentifier.Disconnect, null, false));
                return;
            }

            log.Info("Connected, encrypted, beginning node synchronization.");

            bool librariesSynced = false;
            do
            {
                heartbeat.Stop();
                NodeLibraryManager.Instance.IndexLibraries();
                
                //Serialize for upload
                var libraryIndex = NodeLibraryManager.Instance.FileIndex;
                byte[] serializedIndex;
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, libraryIndex);
                    serializedIndex = ms.ToArray();
                }

                //Build message
                socket.Send(BuildMessage(MessageIdentifier.NodeSync, serializedIndex, true));

                //Wait for response
                var resp = socket.ReceiveMessage();

                var data = DecryptMessage(resp.First.Buffer.Skip(1).ToArray());
                
                if(resp.First.Buffer[0] == (byte)MessageIdentifier.NodeSync)
                {
                    //Perform some synchronization job
                    heartbeat.Start();
                    NodeLibraryManager.Instance.PerformSyncJob(
                        Serializer.Deserialize<SyncJob>(new MemoryStream(data)));
                    heartbeat.Stop();
                }else
                {
                    librariesSynced = true;
                }

            } while (!librariesSynced);
            NodeManager manager = new NodeManager("CompiledNodes");

            manager.Initialize();
            manager.LogLoadedModules();

            socket.Send(BuildMessage(MessageIdentifier.BeginOperation, null, true));

            log.Info("Connection and sync procedure complete, commencing operation.");
            heartbeat.Start();
            RequestNodeList();

            while(status == 1)
            {
                msg = socket.ReceiveMessage(TimeSpan.FromMilliseconds(50));
                if (msg.FrameCount == 0) continue;

                if((MessageIdentifier)msg.First.Buffer[0] != MessageIdentifier.Heartbeat) 
                    log.Debug("Received message: " + Enum.GetName(typeof(MessageIdentifier), msg.First.Buffer[0]));
                var data = DecryptMessage(msg.First.Buffer.Skip(1).ToArray());
                switch((MessageIdentifier)msg.First.Buffer[0])
                {
                    case MessageIdentifier.NodeVerify:
                        var reqId = BitConverter.ToInt32(data, 0);
                        var response = BitConverter.ToBoolean(data, sizeof (int));
                        NodeExistsResponses.Add(reqId, response);
                        break;
                    case MessageIdentifier.LaunchNode:
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            var nodeInfo = Serializer.Deserialize<NodeInfo>(ms);
                            Task.Factory.StartNew(()=>NodePool.Instance.LaunchNode(nodeInfo));
                        }
                        break;
                    case MessageIdentifier.ShutdownNode:
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            var nodeInfo = Serializer.Deserialize<NodeInfo>(ms);
                            Task.Factory.StartNew(()=>NodePool.Instance.ShutdownNode(nodeInfo));
                        }
                        break;
                    case MessageIdentifier.RMIInvoke:
                        NodeRMI rmi;
                        using(MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            rmi = Serializer.Deserialize<NodeRMI>(ms);
                        }
                        Task.Factory.StartNew(()=>NodePool.Instance.ProcessRMI(rmi));
                        break;
                    case MessageIdentifier.RMIResponse:
                        NodeRMI rmir;
                        using(MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            rmir = Serializer.Deserialize<NodeRMI>(ms);
                        }
                        NodePool.Instance.rmiResponses.Add(rmir.RequestID, rmir);
                        break;
                    case MessageIdentifier.Heartbeat:
                        heartbeatAttempts = 0;
                        break;
                    case MessageIdentifier.ReqNodeList:
                        NodeDictionary.Clear();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            var nodeInfo = Serializer.Deserialize<NodeInfo[]>(ms);
                            foreach(var info in nodeInfo)
                            {
                                info.RMIResolvedType =
                                    NodeManager.Instance.GetHandlerForRMITypeName(info.RMITypeName).ComponentModel.Implementation;
                                NodeDictionary.Add(info.Id, info);
                            }
                            log.Debug("Received node dictionary rebuild with "+nodeInfo.Length+" entries.");
                        }
                        break;
                    case MessageIdentifier.NodeRemoved:
                    case MessageIdentifier.NodeAdded:
                        using(MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            var nodeInfo = Serializer.Deserialize<NodeInfo>(ms);
                            if ((MessageIdentifier)msg.First.Buffer[0] == MessageIdentifier.NodeAdded)
                            {
                                nodeInfo.RMIResolvedType =
                                    NodeManager.Instance.GetHandlerForRMITypeName(nodeInfo.RMITypeName).ComponentModel.Implementation;
                                NodeDictionary.Add(nodeInfo.Id, nodeInfo);
                            }
                            else
                                NodeDictionary.Remove(nodeInfo.Id);
                        }
                        break;
                    default:
                        log.Error("Unknown message received...");
                        break;
                }
            }
        }

        /// <summary>
        /// Request a complete refresh of the node list.
        /// </summary>
        public void RequestNodeList()
        {
            socket.Send(BuildMessage(MessageIdentifier.ReqNodeList, null, true));
        }

        /// <summary>
        /// Decrypt an encrypted message.
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        private byte[] DecryptMessage(byte[] encrypted)
        {
            return encryption.Decrypt(encrypted);
        }

        private void LogUnexpectedMessage(byte[] buffer)
        {
            log.Error("Unexpected message: "+Enum.GetName(typeof(MessageIdentifier), buffer[0]));
        }

        /// <summary>
        /// Build a message based on an identifier and a data array.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] BuildMessage(MessageIdentifier message, byte[] data, bool encryp)
        {
            byte[] combinedMessage;
            if (data == null)
                combinedMessage = new byte[1];
            else
            {
                //Encrypt the data if needed
                byte[] finalData = encryp ? encryption.Encrypt(data) : data;
                combinedMessage = new byte[finalData.Length + 1];
                finalData.CopyTo(combinedMessage, 1);
            }
            combinedMessage[0] = (byte)message;
            return combinedMessage;
        }
        
        /// <summary>
        /// Send the request for the URL to the library for downloading.
        /// </summary>
        /// <param name="nodeLibDownloader"></param>
        public string RequestLibraryURL(string library, int reqId, NodeLibraryManager nodeLibDownloader)
        {
            socket.Send(BuildMessage(MessageIdentifier.GetLibraryURL, Encoding.Unicode.GetBytes(reqId+":"+library), true));
            var message = socket.ReceiveMessage();
            //todo: check if it actually is a url, this is really hacky for now
            var data = Encoding.UTF8.GetString(DecryptMessage(message.First.Buffer.Skip(1).ToArray()));
            var url = data.Split(':')[1];
            return url;
        }

        /// <summary>
        /// Verify the NodeInfo identifier exists and impliments RMI type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool AsyncNodeVerify(Type type, NodeInfo identifier)
        {
            int randId = new Random().Next();
            using (var data = new MemoryStream())
            {
                Serializer.Serialize(data, identifier);
                var reqId = BitConverter.GetBytes(randId);
                var identifierBytes = data.ToArray();
                byte[] finalData = new byte[reqId.Length+identifierBytes.Length];
                reqId.CopyTo(finalData, 0);
                identifierBytes.CopyTo(finalData, identifierBytes.Length);
                byte[] message = BuildMessage(MessageIdentifier.NodeVerify, identifierBytes, true);
                socket.Send(message);
            }
            
            while(!NodeExistsResponses.ContainsKey(randId))
            {
                Thread.Sleep(100);
            }

            var response = NodeExistsResponses[randId];
            NodeExistsResponses.Remove(randId);
            return response;
        }

        /// <summary>
        /// Send out a RMI response.
        /// </summary>
        /// <param name="rmi"></param>
        public void ProcessRMIResponse(NodeRMI rmi)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, rmi);
                socket.Send(BuildMessage(MessageIdentifier.RMIResponse, ms.ToArray(), true));
            }
        }

        public void SendTo(object info, object buildMessage)
        {
            
        }

        public NodeInfo NodeForId(int nodeID)
        {
            return NodeDictionary[nodeID];
        }

        public void Send(byte[] message)
        {
            socket.Send(message);
        }

        public NodeInfo NodeForType<T>()
        {
            return NodeDictionary.Values.SingleOrDefault(e => e.RMITypeName == typeof (T).FullName);
        }

        public NodeInfo[] AllNodeForType<T>()
        {
            return NodeDictionary.Values.Where(e => e.RMITypeName == typeof (T).FullName).ToArray();
        }
    }
}
