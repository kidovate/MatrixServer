using System;
using System.Linq;
using MMOController.Data;
using MatrixAPI.Encryption;
using log4net;
using System.Timers;
using MatrixAPI;
using MMOCommon;
using System.IO;
using System.Text;
using ProtoBuf;
using MatrixAPI.Util;

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
		private readonly ILog log;
		private string passwordSalt;
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
			log = LogManager.GetLogger("CLIENT "+this.GetHashCode().ToString().Truncate(4));
			clientInfo = new ClientInfo() {Id = ClientCache.RandomId()};
			status = ClientStatus.NoIdentity;
			passwordSalt = Path.GetRandomFileName();
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

			//if(message[0] != (byte)MessageIdentifier.Heartbeat) 
				//log.Debug("Received message: "+System.Enum.GetName(typeof(MessageIdentifier), message[0]));
            switch ((MessageIdentifier)message[0])
            {
			case MessageIdentifier.Heartbeat:
				clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.Heartbeat, null));
				break;
			case MessageIdentifier.SetIdentity:
				if (status != ClientStatus.NoIdentity)
				{
					break;
				}
				log.Debug("Setting up encryption with new client...");
				status = ClientStatus.NoEncryption;
				//Begin encryption exchange
				clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.BeginEncryption, null));
				break;
				//Coming from client, this is the encryption md5.
			case MessageIdentifier.BeginEncryption:
				if (status != ClientStatus.NoEncryption)
				{
					log.Error("Unexpected BeginEncryption from host.");
					break;
				}
				//Get the encryption key MD5.
				byte[] keymd5 = message.Skip(1).ToArray();
				AES encrypt = MmoEncrypt.ByHash(keymd5);
				if (encrypt == null)
				{
					log.Info("Key not valid for client, rejecting and disconnecting.");
					clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.InvalidKey, null));
					clientInter.Disconnect(clientInfo);
					status = ClientStatus.Disconnected;
				}
				else
				{
					log.Debug("Client accepted, beginning login.");
					status = ClientStatus.LoggingIn;
					clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.ConfirmEncryption, Encoding.UTF8.GetBytes(passwordSalt)));
					encryption = encrypt;
				}
				break;
			case MessageIdentifier.LoginVerify:
				if(status != ClientStatus.LoggingIn)
				{	
					log.Error("Client tried to log in when not in login state.");
					break;
				}
				LoginRequest request = message.Deserialize<LoginRequest>();
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
				byte[] finalData = status > ClientStatus.NoEncryption ? encryption.Encrypt(data) : data;
				combinedMessage = new byte[finalData.Length+1];
				finalData.CopyTo(combinedMessage, 1);
			}
			combinedMessage[0] = (byte) message;
			return combinedMessage;
		}
	}
}

