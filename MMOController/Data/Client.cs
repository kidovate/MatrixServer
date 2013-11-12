using System;
using MatrixAPI.Encryption;
using log4net;
using System.Timers;

namespace MMOController
{
	/// <summary>
	/// A handler for a game client with a long heartbeat (for less lag)
	/// </summary>
	public class Client
	{
		private ClientInfo clientInfo;
		private ClientStatus status;
		private AES encryption;
		private static readonly ILog log = LogManager.GetLogger(typeof(Client));
		Timer heartbeat = new Timer(30000);
		private LoginNode clientInter;
		//todo: add zones collection

		/// <summary>
		/// Returns the identifier, a 27 byte array.
		/// </summary>
		public byte[] Id
		{
			get { return clientInfo.Id; }
		}

		/// <summary>
		/// Host information.
		/// </summary>
		public ClientInfo Info
		{
			get { return clientInfo; }
		}

		public Client ()
		{
			clientInfo = new ClientInfo() {Id = ClientCache.RandomId()};
			status = ClientStatus.NoIdentity;
			heartbeat.Elapsed += (sender, args) =>
			{
				log.Info("Client timed out, terminating");
				status = HostStatus.Disconnected;
				lastInter.DisconnectHost(hostInfo);
				heartbeat.Stop();
				heartbeat.Dispose();
				LeaveAllZones();
			};
		}

		/// <summary>
		/// Shortcut - tells all zones the client has left the network.
		/// </summary>
		void LeaveAllZones(){
		}
	}
}

