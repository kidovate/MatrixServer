using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using MMOController.Data;
using MMOController.Model.Accounts;
using MMOController.Properties;
using MatrixAPI.Interfaces;
using ProtoBuf;
using log4net;
using ZeroMQ;
using MatrixAPI.Data;
using MMOCommon;

namespace MMOController
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

		/// <summary>
		/// Initialize the node
		/// </summary>
		/// <param name="portal">access portal to the Matrix. should be stored.</param>
		public void Initialize(IMatrixPortal portal)
		{
			log.Info("Launching a new game server node on port "+Settings.Default.GameInterfacePort);
			server = MmoZmq.context.CreateSocket(ZeroMQ.SocketType.ROUTER);

			status = 1;
			serverTask = Task.Factory.StartNew(ServerThread);
		}

		void ServerThread()
		{
			server.Bind(Settings.Default.Protocol+"://*:" + Settings.Default.GameInterfacePort);
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
						//Setup a client object for this.
						var client = new Client(this);
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
					//Find the host object.
					var hostIdentity = ClientCache.FindClient(identity.Buffer);
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
						hostIdentity.ProcessMessage(data.Buffer);
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

