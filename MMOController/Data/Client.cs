using System;
using System.Linq;
using MMOController.Data;
using MatrixAPI.Encryption;
using MatrixAPI.Enum;
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

		public Client (LoginNode inter)
		{
			clientInfo = new ClientInfo() {Id = ClientCache.RandomId()};
			status = ClientStatus.NoIdentity;
			heartbeat.Elapsed += (sender, args) =>
			{
				log.Info("Client timed out, terminating");
				status = ClientStatus.Disconnected;
				clientInter.Disconnect(clientInfo);
				heartbeat.Stop();
				heartbeat.Dispose();
				LeaveAllZones();
			};
		    clientInter = inter;
		}

		/// <summary>
		/// Shortcut - tells all zones the client has left the network.
		/// </summary>
		void LeaveAllZones(){
		}

	    public void ProcessMessage(byte[] message)
	    {
	        if (status == ClientStatus.Disconnected)
                return;

            //Reset heartbeat
            heartbeat.Stop();
            heartbeat.Start();

            if(status > ClientStatus.NoEncryption && message.Length > 1)
            {
                //Decrypt message
                var decrypt = encryption.Decrypt(message.Skip(1).ToArray());
                byte[] finalMessage = new byte[1+decrypt.Length];
                decrypt.CopyTo(finalMessage,1);
                finalMessage[0] = message[0];
                message = finalMessage;
            }

            if(message[0] != (byte)MessageIdentifier.Heartbeat) 
                log.Debug("Received message: "+System.Enum.GetName(typeof(MessageIdentifier), message[0]));
            switch ((MessageIdentifier)message[0])
            {
            }
	    }
	}
}

