using System;
using System.Collections.Generic;
using System.Linq;
using MMOController.Data;
using MMOController.Model.Accounts;
using MMOController.Nodes;
using MatrixAPI.Encryption;
using NHibernate;
using NHibernate.Criterion;
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
		Timer heartbeat = new Timer(20000);
	    private User thisUser;
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
			    DisconnectClient();
			};
		    clientInter = inter;
		}

	    private void DisconnectClient()
	    {
            status = ClientStatus.Disconnected;
            clientInter.Disconnect(clientInfo);
            heartbeat.Stop();
            heartbeat.Dispose();
            LeaveAllZones();
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
			case MessageIdentifier.Heartbeat:
				//clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.Heartbeat, null));
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
                    encryption = encrypt;
					clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.ConfirmEncryption, Encoding.UTF8.GetBytes(passwordSalt)));
				}
				break;
			case MessageIdentifier.LoginVerify:
				if(status != ClientStatus.LoggingIn)
				{	
					log.Error("Client tried to log in when not in login state.");
					break;
				}
                
				LoginRequest request = message.Skip(1).ToArray().Deserialize<LoginRequest>();
                log.Debug("Login request, "+request.Username);
                User user; 
                using(ISession session= MmoDatabase.Session)
                {
                    using(var transaction = session.BeginTransaction())
                    {
                        user = session.CreateCriteria(typeof (User))
                            .Add(Restrictions.Eq("Username", request.Username))
                            .UniqueResult<User>();
                    }
                }
                LoginResponse response;
                if(user == null)
                {
                    response = new LoginResponse() {Success = false, Message = "User does not exist."};
                }else
                {
                    string hashedPassword = StringMD5.CreateMD5Hash(StringMD5.CreateMD5Hash(passwordSalt + user.Password)+passwordSalt);
                    if (request.MD5Pass != hashedPassword)
                    {
                        response = new LoginResponse() { Success = false, Message = "Password is incorrect." };
                    }
                    else
                    {
                        //Here we should actually be logged in
                        log.Debug("Client logged in, username: " + user.Username);
                        thisUser = user;
                        status = ClientStatus.CharacterSelect;
                        response = new LoginResponse() {Success = true};
                    }
                }

                clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.LoginVerify, response.Serialize()));
				break;
            case MessageIdentifier.Disconnect:
                    DisconnectClient();
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

