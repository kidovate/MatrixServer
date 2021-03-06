﻿using System;
using System.IO;
using System.Threading;
using MatrixAPI.Data;
using MatrixAPI.Enum;
using MatrixAPI.Interfaces;
using MatrixMaster.Data;
using MatrixMaster.Nodes;
using ProtoBuf;
using ZeroMQ;
using log4net;
using MatrixMaster.Properties;
using MatrixAPI.Util;

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
        public static HostInterface Instance;
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
            var controllerPortal = new ControllerPortal();
            controllerPortal.SetNodeID(0);
            controller.Initialize(controllerPortal);
            Instance = this;
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
			server.Bind(Settings.Default.Protocol+"://*:"+port);
            serverThread = new Thread(ServerThread);
            serverThread.Start();
        }

        /// <summary>
        /// B
        /// locking thread to receive and process data.
        /// </summary>
        public void ServerThread()
        {
            status = 1;

            while(status == 1)
            {

                var message = server.ReceiveMessage(TimeSpan.FromMilliseconds(500));
                if (message.FrameCount == 0) continue;

                //Determine the identity of the host
                var identity = message[0];
                var data = message[1];

                //Initial connection will be a 15 byte identity.
                if(identity.BufferSize == 15)
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
                        lock (server)
                        {
                            server.SendMore(identity.Buffer);
                            server.Send(new byte[] {(byte) MessageIdentifier.InvalidIdentity});
                        }
                    }else
                    {
                        hostIdentity.ProcessMessage(this, data.Buffer);
                    }

                }
            }
			server.Unbind(Settings.Default.Protocol+"://*:"+port);
        }

        public void Shutdown()
        {
            status = 0;
        }

        /// <summary>
        /// Send a message to the host.
        /// </summary>
        /// <param name="hostInfo">Host identifier.</param>
        /// <param name="buildMessage">Message to send.</param>
        public void SendTo(HostInfo hostInfo, byte[] buildMessage)
        {
            lock (server)
            {
                server.SendMore(hostInfo.Id);
                server.Send(buildMessage);
            }
        }

        /// <summary>
        /// Disconnect the host.
        /// </summary>
        /// <param name="hostInfo"></param>
        public void DisconnectHost(HostInfo hostInfo)
        {
            foreach (var node in HostCache.FindHost(hostInfo.Id).Nodes)
                NodePool.Instance.DestroyNode(node);
            HostCache.ConnectedHosts.Remove(hostInfo.Id);
        }

        /// <summary>
        /// Route a RMI response to the host.
        /// </summary>
        /// <param name="rmi"></param>
        public void RouteRMIResponse(NodeRMI rmi)
        {
            var target = NodePool.Instance.NodeForId(rmi.SNodeID);
            if(target == null)
            {
                log.Debug("RMI response "+rmi.RequestID+" dropped as the initiator doesn't exist anymore.");
                return;
            }
            var targetHost = HostCache.FindHost(target.HostID);
			SendTo(targetHost.Info, targetHost.BuildMessage(MessageIdentifier.RMIResponse, rmi.Serialize()));
        }

        /// <summary>
        /// Route a RMI request to the host with the target node
        /// </summary>
        /// <param name="rmi"></param>
        /// <param name="destoNode"> </param>
        public void RouteRMIRequest(NodeRMI rmi, NodeInfo destoNode)
        {
            if(destoNode.HostID.Length == 1)
            {
                rmi.SerializeReturnValue(destoNode.RMIResolvedType.GetMethod(rmi.MethodName).Invoke(controller, rmi.DeserializeArguments()));
                RouteRMIResponse(rmi);
                return;
            }
            var destoHost = HostCache.FindHost(destoNode.HostID);
			SendTo(destoHost.Info, destoHost.BuildMessage(MessageIdentifier.RMIInvoke, rmi.Serialize()));
        }
    }
}
