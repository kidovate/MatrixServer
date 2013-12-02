using System;
using System.Threading.Tasks;
using MMOController.Data;
using MMOController.Properties;
using MatrixAPI.Interfaces;
using log4net;
using ZeroMQ;
using MMOCommon;

namespace MMOController.Nodes
{
	/// <summary>
	///  Initial and constant connection point for clients (game engine). Maintains login state of a client.
	/// </summary>
	public class LoginNode : INode, ILoginNode
	{
		private ZmqSocket server;
		private static readonly ILog log = LogManager.GetLogger(typeof(LoginNode));
		private Task serverTask;
		private int status = 0;
		private IMMOCluster controller;

		/// <summary>
		/// Initialize the node
		/// </summary>
		/// <param name="portal">access portal to the Matrix. should be stored.</param>
		public void Initialize(IMatrixPortal portal)
		{
			log.Info("Launching a new game server node on port "+Settings.Default.GameInterfacePort);
			server = MmoZmq.context.CreateSocket(ZeroMQ.SocketType.ROUTER);

		    log.Debug(MmoEncrypt.Keys.Count+" encryption keys.");

			controller = portal.GetNodeProxy<IMMOCluster>();

		    log.Debug("Realms count: "+MmoWorld.Realms.Count);

			status = 1;
			serverTask = Task.Factory.StartNew(ServerThread);
		}

		void ServerThread()
		{
			server.Bind(Settings.Default.Protocol+"://*:" + Settings.Default.GameInterfacePort);
			while(status == 1)
			{
				var message = server.ReceiveMessage(TimeSpan.FromMilliseconds(600));
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
						//Setup a client object for this.
						var client = new Client(this, controller);
						ClientCache.RegisterClient(client);
						client.Id.CopyTo(response, 1);
						server.SendMore(identity.Buffer);
						server.Send(response);
					}else
					{
						log.Debug("Client with 1 byte identity, message "+System.Enum.GetName(typeof(MessageIdentifier), data.Buffer[0]));
					}

				}else if(identity.BufferSize == 0)
				{
					log.Error("Empty identity message discarded.");
				}
				else
				{
					//Find the client
					var clientIdentity = ClientCache.FindClient(identity.Buffer);
					if (clientIdentity == null)
					{
						log.Debug("Client contacted with invalid identity!");
						lock (server)
						{
							server.SendMore(identity.Buffer);
							server.Send(new byte[] {(byte) MessageIdentifier.InvalidIdentity});
						}
					}else
					{
						Task.Factory.StartNew(()=>clientIdentity.ProcessMessage(data.Buffer));
					}

				}
			}
			server.Unbind(Settings.Default.Protocol+"://*:"+Settings.Default.GameInterfacePort);
			log.Info("Game handler shut down.");
		}

		/// <summary>
		/// End connections, release everything, and shut down.
		/// </summary>
		public void Shutdown()
		{
			status = 0;
		}

        /// <summary>
        /// Sets a client as disconnected. NOT to be used to kill a client.
        /// </summary>
        /// <param name="clientInfo"></param>
	    public void Disconnect(ClientInfo clientInfo)
        {
            var client = ClientCache.FindClient(clientInfo.Id);
            if (client == null) return;
            ClientCache.ConnectedClients.Remove(client.Id);
        }

		/// <summary>
		/// Send a message to the host.
		/// </summary>
		/// <param name="clientInfo">Client identifier.</param>
		/// <param name="buildMessage">Message to send.</param>
		public void SendTo(ClientInfo clientInfo, byte[] buildMessage)
		{
			lock (server)
			{
				server.SendMore(clientInfo.Id);
				server.Send(buildMessage);
			}
		}
	}
}

