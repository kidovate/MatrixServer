using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MatrixAPI.Data;
using MatrixAPI.Encryption;
using MatrixAPI.Enum;
using MatrixAPI.Exceptions;
using MatrixMaster.Encryption;
using MatrixMaster.Enums;
using MatrixMaster.Nodes;
using MatrixMaster.Servers;
using ProtoBuf;
using log4net;

namespace MatrixMaster.Data
{
    /// <summary>
    /// An individual connected host.
    /// </summary>
    public class Host
    {
        private HostInfo hostInfo;
        private HostStatus status;
        private AES encryption;
        private static readonly ILog log = LogManager.GetLogger(typeof(Host));
        Timer heartbeat = new Timer(30000);
        private HostInterface lastInter;
        private ObservableCollection<NodeInfo> _nodes;

        /// <summary>
        /// Returns the identifier, a 27 byte array.
        /// </summary>
        public byte[] Id
        {
            get { return hostInfo.Id; }
        }

        /// <summary>
        /// Host information.
        /// </summary>
        public HostInfo Info
        {
            get { return hostInfo; }
        }

        /// <summary>
        /// Node information.
        /// </summary>
        public ObservableCollection<NodeInfo> Nodes
        {
            get { return _nodes; }
        } 

        /// <summary>
        /// Create a new instance of a host with a random ID.
        /// </summary>
        public Host()
        {
            hostInfo = new HostInfo() {Id = HostCache.RandomId()};
            status = HostStatus.NoIdentity;
            heartbeat.Elapsed += (sender, args) =>
                                     {
                                         log.Info("Host timed out, terminating");
                                         status = HostStatus.Disconnected;
                                         lastInter.DisconnectHost(hostInfo);
                                         heartbeat.Stop();
                                         heartbeat.Dispose();
                                     };
            NodePool.Instance.Nodes.CollectionChanged += OnGlobalNodesChange;
            _nodes = new ObservableCollection<NodeInfo>();
            _nodes.CollectionChanged += NodesCollectionChanged;
        }

        private void OnGlobalNodesChange(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                SendEntireNodeList();
                return;
            }

            if(notifyCollectionChangedEventArgs.NewItems != null)
            foreach (var newNode in notifyCollectionChangedEventArgs.NewItems)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, ((NodeInfo)newNode));
                    lastInter.SendTo(hostInfo, BuildMessage(MessageIdentifier.NodeAdded, ms.ToArray()));
                }
            }
            if(notifyCollectionChangedEventArgs.OldItems != null)
            foreach (var oldNode in notifyCollectionChangedEventArgs.OldItems)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, (NodeInfo)oldNode);
                    lastInter.SendTo(hostInfo, BuildMessage(MessageIdentifier.NodeRemoved, ms.ToArray()));
                }
            }
        }
        public void SendEntireNodeList()
        {
            using(var ms = new MemoryStream())
            {
                NodeInfo[] nodes = NodePool.Instance.GetNodeList();
                Serializer.Serialize(ms, nodes);
                lastInter.SendTo(hostInfo, BuildMessage(MessageIdentifier.ReqNodeList, ms.ToArray()));
            }
        }

    /// <summary>
        /// Monitor the Nodes array.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NodesCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch(args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    using(MemoryStream ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, args.NewItems[0]);
                        lastInter.SendTo(hostInfo, BuildMessage(MessageIdentifier.LaunchNode, ms.ToArray()));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        NodePool.Instance.DestroyNode((NodeInfo) args.OldItems[0]);
                        Serializer.Serialize(ms, (NodeInfo) args.OldItems[0]);
                        lastInter.SendTo(hostInfo, BuildMessage(MessageIdentifier.ShutdownNode, ms.ToArray()));
                    }
                    break;
            }
        }

        /// <summary>
        /// Process a message for response.
        /// </summary>
        /// <param name="inter">Interface.</param>
        /// <param name="message">Received message.</param>
        public void ProcessMessage(HostInterface inter, byte[] message)
        {
            lastInter = inter;
            if (status == HostStatus.Disconnected)
                return;

            //Reset heartbeat
            heartbeat.Stop();
            heartbeat.Start();

            if(status > HostStatus.NoEncryption && message.Length > 1)
            {
                //Decrypt message
                var decrypt = encryption.Decrypt(message.Skip(1).ToArray());
                byte[] finalMessage = new byte[1+decrypt.Length];
                decrypt.CopyTo(finalMessage,1);
                finalMessage[0] = message[0];
                message = finalMessage;
            }

            if(message[0] != (byte)MessageIdentifier.Heartbeat) 
                log.Debug("Received message: "+Enum.GetName(typeof(MessageIdentifier), message[0]));
            switch((MessageIdentifier)message[0])
            {
                case MessageIdentifier.Heartbeat:
                    inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.Heartbeat, null));
                    break;
                    //Coming from client, this is confirming an identity.
                case MessageIdentifier.SetIdentity:
                    if (status != HostStatus.NoIdentity)
                    {
                        log.Debug("Host already has registered.");
                        break;
                    }
                    log.Debug("Identity confirmed, beginning encryption sequence.");
                    status = HostStatus.NoEncryption;
                    //Begin encryption exchange
                    inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.BeginEncryption, null));
                    break;
                    //Coming from client, this is the encryption md5.
                case MessageIdentifier.BeginEncryption:
                    if (status != HostStatus.NoEncryption)
                    {
                        log.Error("Unexpected BeginEncryption from host.");
                        break;
                    }
                    //Get the encryption key MD5.
                    byte[] keymd5 = message.Skip(1).ToArray();
                    log.Debug("Encryption key confirmation request: " +
                              BitConverter.ToString(keymd5).Replace("-", "").ToLower());
                    AES encrypt = EncryptionKeyDB.Instance.ByHash(keymd5);
                    if (encrypt == null)
                    {
                        log.Info("Key not valid for host, rejecting and disconnecting.");
                        inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.InvalidKey, null));
                        inter.DisconnectHost(hostInfo);
                        status = HostStatus.Disconnected;
                    }
                    else
                    {
                        log.Debug("Host accepted, beginning node sync.");
                        status = HostStatus.SyncNodes;
                        inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.ConfirmEncryption, null));
                        encryption = encrypt;
                    }
                    break;
                case MessageIdentifier.NodeSync:
                    var inputIndex =
                        Serializer.Deserialize<Dictionary<string, byte[]>>(new MemoryStream(message.Skip(1).ToArray()));
                    var syncJob = NodeLibraryManager.Instance.CreateSyncJob(inputIndex);
                    if (syncJob == null)
                    {
                        inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.BeginOperation, null));
                        status = HostStatus.LoadingNodes;
                    }
                    else
                    {
                        byte[] serializedJob;
                        using (var ms = new MemoryStream())
                        {
                            Serializer.Serialize(ms, syncJob);
                            serializedJob = ms.ToArray();
                            status = HostStatus.SyncNodes;
                        }
                        inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.NodeSync, serializedJob));
                    }
                    break;
                case MessageIdentifier.GetLibraryURL:
                    //Retreive the library url for the path
                    string dataString = Encoding.Unicode.GetString(message, 1, message.Length - 1);
                    int reqId = int.Parse(dataString.Split(':')[0]);
                    string library = dataString.Split(':')[1];
                    var libraryUrl = Encoding.UTF8.GetBytes(reqId + ":" + NodeHost.Instance.GetDownloadUrl(library));
                    inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.GetLibraryURL, libraryUrl));
                    break;
                case MessageIdentifier.BeginOperation:
                    if (status == HostStatus.LoadingNodes)
                    {
                        status = HostStatus.Operating;
                        log.Info("New host ready for operation.");
                        try
                        {
                            Task.Factory.StartNew(() => inter.Controller.OnHostAdded(hostInfo));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error not caught by Controller, " + ex.ToString());
                        }
                    }
                    break;
                case MessageIdentifier.NodeVerify:
                    int verId = BitConverter.ToInt32(message, 1);
                    NodeInfo info;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var inputBytes = message.Skip(1 + sizeof (int)).ToArray();
                        ms.Write(inputBytes, 0, inputBytes.Length);
                        ms.Position = 0;
                        info = Serializer.Deserialize<NodeInfo>(ms);
                    }

                    var response = NodePool.Instance.CheckNodeExists(info);
                    var respBytes = new byte[sizeof (int) + sizeof (bool)];
                    BitConverter.GetBytes(verId).CopyTo(respBytes, 0);
                    BitConverter.GetBytes(response).CopyTo(respBytes, sizeof (int));
                    inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.NodeVerify, respBytes));
                    break;
                case MessageIdentifier.RMIResponse:
                    if (status != HostStatus.Operating) return;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var data = message.Skip(1).ToArray();
                        ms.Write(data, 0, data.Length);
                        ms.Position = 0;
                        var rmi = Serializer.Deserialize<NodeRMI>(ms);
                        var destoNode = NodePool.Instance.NodeForId(rmi.SNodeID);
                        if (destoNode == null) return;
                        if (destoNode.Id == 0) NodePool.Instance.HandleRMIResponse(rmi);
                        else
                        {
                            lastInter.RouteRMIResponse(rmi);
                        }
                    }
                    break;
                case MessageIdentifier.ReqNodeList:
                    if (status != HostStatus.Operating) return;
                    SendEntireNodeList();
                    break;
                case MessageIdentifier.RMIInvoke:
                    {
                        if (status != HostStatus.Operating) return;
                        NodeRMI rmi;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            var data = message.Skip(1).ToArray();
                            ms.Write(data, 0, data.Length);
                            ms.Position = 0;
                            rmi = Serializer.Deserialize<NodeRMI>(ms);
                        }
                        var destoNode = NodePool.Instance.NodeForId(rmi.NodeID);
                        if (destoNode == null)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                rmi.SerializeReturnValue(new NodeNotExistException());
                                Serializer.Serialize(ms, rmi);
                                lastInter.SendTo(hostInfo, BuildMessage(MessageIdentifier.RMIResponse, ms.ToArray()));
                                break;
                            }
                        }
                        lastInter.RouteRMIRequest(rmi, destoNode);
                    }
                    break;
            }
        }

        /// <summary>
        /// Build a message based on an identifier and a data array.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] BuildMessage(MessageIdentifier message, byte[] data)
        {
            byte[] combinedMessage;
            if(data == null)
                combinedMessage = new byte[1];
            else
            {
                //Encrypt the data if needed
                byte[] finalData = status > HostStatus.NoEncryption ? encryption.Encrypt(data) : data;
                combinedMessage = new byte[finalData.Length+1];
                finalData.CopyTo(combinedMessage, 1);
            }
            combinedMessage[0] = (byte) message;
            return combinedMessage;
        }

    }
}
