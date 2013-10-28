using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MatrixAPI.Encryption;
using MatrixAPI.Enum;
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

        private AES encryption;

        private byte[] keyHash;

        /// <summary>
        /// Create a new host client.
        /// </summary>
        /// <param name="serverIp">Master server IP</param>
        /// <param name="serverPort">Master server port.</param>
        public HostClient(string serverIp, int serverPort, AES encryption, byte[] keyHash)
        {
            log.Info("Creating a new HostClient.");
            this.encryption = encryption;
            this.keyHash = keyHash;
            context = ZmqContext.Create();
            socket = context.CreateSocket(SocketType.DEALER);
            socket.TcpKeepalive = TcpKeepaliveBehaviour.Enable;
            masterIp = serverIp;
            masterPort = serverPort;
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
                byte[] identity = new byte[1];
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
                socket.Send(new byte[] { (byte)MessageIdentifier.Disconnect });
                return;
            }

            log.Info("Connected, encrypted, and ready to launch nodes.");

            while(status == 1)
            {
                msg = socket.ReceiveMessage();

                log.Debug("Received message: " + Enum.GetName(typeof(MessageIdentifier), msg.First.Buffer[0]));
            }
        }

        private void LogUnexpectedMessage(byte[] buffer)
        {
            log.Error("Unexpected message: "+Enum.GetName(typeof(MessageIdentifier), buffer[0]));
        }
    }
}
