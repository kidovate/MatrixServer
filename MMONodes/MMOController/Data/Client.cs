using System;
using System.Collections.Generic;
using System.Linq;
using MMOController.Data;
using MMOController.Model.Accounts;
using MMOController.Nodes;
using MMOController.Model.Character;
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
		private Character selectedCharacter;
		private AES encryption;
		private ILog log; //Updated when the client logs in 
		private string passwordSalt;
		Timer heartbeat = new Timer(20000);
	    private User thisUser;
		IMMOCluster controller;
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

		public Client (LoginNode inter, IMMOCluster cluster)
		{
			controller = cluster;
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
			if(thisUser != null){
				controller.OnUserLoggedOff(thisUser.Username.ToLower());
			}
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

			var data = message.Skip(1).ToArray();

			if(message[0] != (byte)MessageIdentifier.Heartbeat) 
				log.Debug("Received message: "+System.Enum.GetName(typeof(MessageIdentifier), message[0]));
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
				byte[] keymd5 = data;
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
                
				LoginRequest request = data.Deserialize<LoginRequest>();
                log.Debug("Login request, "+request.Username);
                User user;
                ISession session = MmoDatabase.Session;
                using(var transaction = session.BeginTransaction())
                {
                    user = session.CreateCriteria(typeof (User))
                        .Add(Restrictions.Eq("Username", request.Username))
                        .UniqueResult<User>();
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
						//check if the user is logged in
						if(controller.IsUserLoggedIn(user.Username)){
							log.Error("User '"+user.Username.Truncate(5)+"' is already logged in, rejecting...");
							response = new LoginResponse() { Success = false, Message = "User is already logged in!" };
						}else{
	                        //Here we should actually be logged in
	                        log.Debug("Client logged in, username: " + user.Username);
							controller.OnUserLoggedIn(user.Username);
							log = LogManager.GetLogger("CLIENT "+user.Username.Truncate(4));
	                        thisUser = user;
	                        status = ClientStatus.CharacterSelect;
	                        response = new LoginResponse() {Success = true};
						}
                    }
				}
                

                clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.LoginVerify, response.Serialize()));
				break;
			case MessageIdentifier.CharacterData:
				if(status != ClientStatus.CharacterSelect){
					log.Error("Client tried to retreive characters while not in character select state.");
					break;
				}
                if(thisUser.Characters.Count == 0)
                {
                    thisUser.Characters.Add(new Character()
                                                {Gender = true, Name = "Test Character"+(new Random().Next(0,100)), User = thisUser, XP = 5000, CurrentRealm = MmoWorld.Realms.First()});
                    MmoDatabase.Save(thisUser);
                }
				CharacterData[] characters = new CharacterData[thisUser.Characters.Count];
				int i = 0;
				foreach(var character in thisUser.Characters)
				{
				    if (character.CurrentRealm == null) {
                        character.CurrentRealm = MmoWorld.Realms.First();
                        MmoDatabase.Save(character);
				    }
                    characters[i] = new CharacterData(){Id = character.Id, Name = character.Name, XP = character.XP, Gender=character.Gender, Realm = character.CurrentRealm.Name};
						i++;
				}
				clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.CharacterData, characters.Serialize()));
				break;
			case MessageIdentifier.CharacterVerify:
				if(status != ClientStatus.CharacterSelect){
					log.Error("Client tried to select a character while not in character select state.");
					break;
				}
				int characterId = BitConverter.ToInt32(data, 0);
				Character selChar = thisUser.Characters.SingleOrDefault(x=>x.Id == characterId);
				clientInter.SendTo(clientInfo, BuildMessage(MessageIdentifier.CharacterVerify, BitConverter.GetBytes(selChar != null)));
				if(selChar != null){
					//todo: do something after logging in and selecting character
				}
				selectedCharacter = selChar;
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

