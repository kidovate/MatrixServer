using System;
using System.Threading;
using MatrixAPI.Data;
using MatrixAPI.Enum;
using MatrixAPI.Interfaces;
using MatrixMaster.Data;
using ZeroMQ;
using log4net;

namespace MatrixMaster.Servers
{
    /// <summary>
    /// The server to deal with hosts.
    /// </summary>
    public class HostInterface
    {
        private ZmqContext context;
        private ZmqSocket server;
        private int port;
        private Thread serverThread;
        private static readonly ILog log = LogManager.GetLogger(typeof(HostInterface));
        private int status = 0;
        /// <summary>
        /// Create a new Host Interface server.
        /// </summary>
        /// <param name="port">Port to bind to.</param>
        public HostInterface(int port, INodeController controller)
        {
            log.Info("Host server interface launching...");
            this.port = port;
            context = ZmqContext.Create();
            server = context.CreateSocket(SocketType.ROUTER);
            server.TcpKeepalive = TcpKeepaliveBehaviour.Enable;
            this.controller = controller;
            controller.Initialize(new ControllerPortal());
        }

        private INodeController controller;
        /// <summary>
        /// Node controller.
        /// </summary>
        public INodeController Controller
        {
            get { return controller; }
        }

        /// <summary>
        /// Bind and launch the server.
        /// </summary>
        public void Startup()
        {
            server.Bind("tcp://*:"+port);
            serverThread = new Thread(ServerThread);
            serverThread.Start();
        }

        /// <summary>
        /// Blocking thread to receive and process data.
        /// </summary>
        public void ServerThread()
        {
            status = 1;

            while(status == 1)
            {

                var message = server.ReceiveMessage();
                
                //Determine the identity of the host
                var identity = message[0];
                var data = message[1];

                //Initial connection will be a single byte identity.
                if(identity.BufferSize == 1)
                {
                    //This person has no identity. Check for hello message
                    if(data.Buffer[0] == (byte)MessageIdentifier.Init)
                    {
                        //Response message
                        var response = new byte[28];
                        response[0] = (byte)MessageIdentifier.SetIdentity;
                        log.Debug("Assigned new client a 27 byte identity.");
                        //Setup a host object for this.
                        var host = new Host();
                        HostCache.RegisterHost(host);
                        host.Id.CopyTo(response, 1);
                        server.SendMore(identity.Buffer);
                        server.Send(response);
                    }else
                    {
                        log.Debug("Client with 1 byte identity, message "+Enum.GetName(typeof(MessageIdentifier), data.Buffer[0]));
                    }

                }else if(identity.BufferSize == 0)
                {
                    log.Error("Empty identity message discarded.");
                }
                else
                {
                    //Find the host object.
                    var hostIdentity = HostCache.FindHost(identity.Buffer);
                    if (hostIdentity == null)
                    {
                        log.Debug("Client contacted with invalid identity!");
                        server.SendMore(identity.Buffer);
                        server.Send(new byte[] {(byte) MessageIdentifier.InvalidIdentity});
                    }else
                    {
                        hostIdentity.ProcessMessage(this, data.Buffer);
                    }

                }
            }
            server.Unbind("tcp://*:"+port);
        }

        public void Shutdown()
        {
            status = 0;
            serverThread.Abort();
            server.Unbind("tcp://*:" + port);
        }

        /// <summary>
        /// Send a message to the host.
        /// </summary>
        /// <param name="hostInfo">Host identifier.</param>
        /// <param name="buildMessage">Message to send.</param>
        public void SendTo(HostInfo hostInfo, byte[] buildMessage)
        {
            server.SendMore(hostInfo.Id);
            server.Send(buildMessage);
        }

        /// <summary>
        /// Disconnect the host.
        /// </summary>
        /// <param name="hostInfo"></param>
        public void DisconnectHost(HostInfo hostInfo)
        {
            HostCache.ConnectedHosts.Remove(hostInfo.Id);
            //todo: node cache, remove the associated nodes
        }
    }
}
