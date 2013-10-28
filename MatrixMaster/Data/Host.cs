using System;
using System.Linq;
using MatrixAPI.Data;
using MatrixAPI.Encryption;
using MatrixAPI.Enum;
using MatrixMaster.Encryption;
using MatrixMaster.Enums;
using MatrixMaster.Exceptions;
using MatrixMaster.Servers;
using log4net;

namespace MatrixMaster.Data
{
    /// <summary>
    /// An individual connected host.
    /// </summary>
    public class Host
    {
        private HostInfo hostInfo;
        private HostStatus status;
        private AES encryption;
        private static readonly ILog log = LogManager.GetLogger(typeof(Host));
        /// <summary>
        /// Returns the identifier, a 27 byte array.
        /// </summary>
        public byte[] Id
        {
            get { return hostInfo.Id; }
        }

        /// <summary>
        /// Host information.
        /// </summary>
        public HostInfo Info
        {
            get { return hostInfo; }
        }

        /// <summary>
        /// Create a new instance of a host with a random ID.
        /// </summary>
        public Host()
        {
            hostInfo = new HostInfo() {Id = HostCache.RandomId()};
            status = HostStatus.NoEncryption;
        }

        /// <summary>
        /// Process a message for response.
        /// </summary>
        /// <param name="inter">Interface.</param>
        /// <param name="message">Received message.</param>
        public void ProcessMessage(HostInterface inter, byte[] message)
        {
            if (status == HostStatus.Disconnected)
                return;

           // if(!Enum.IsDefined(typeof(HostInterface), message[0]))
           //     throw new InvalidMessageException();

            if(status == HostStatus.Operating && message.Length > 1)
            {
                //Decrypt message
                var decrypt = encryption.Decrypt(message.Skip(1).ToArray());
                byte[] finalMessage = new byte[1+decrypt.Length];
                decrypt.CopyTo(finalMessage,1);
                finalMessage[0] = message[0];
                message = finalMessage;
            }

            switch((MessageIdentifier)message[0])
            {
                //Coming from client, this is confirming an identity.
                case MessageIdentifier.SetIdentity:
                    log.Debug("Identity confirmed, beginning encryption sequence.");
                    status = HostStatus.NoEncryption;
                    //Begin encryption exchange
                    inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.BeginEncryption, null));
                    break;
                //Coming from client, this is the encryption md5.
                case MessageIdentifier.BeginEncryption:
                    //Get the encryption key MD5.
                    byte[] keymd5 = message.Skip(1).ToArray();
                    log.Debug("Encryption key confirmation request: "+BitConverter.ToString(keymd5).Replace("-","").ToLower());
                    AES encrypt = EncryptionKeyDB.Instance.ByHash(keymd5);
                    if(encrypt == null)
                    {
                        log.Info("Key not valid for host, rejecting and disconnecting.");
                        inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.InvalidKey, null));
                        inter.DisconnectHost(hostInfo);
                        status = HostStatus.Disconnected;
                    }
                    else
                    {
                        log.Debug("Host accepted, beginning encrypted communications.");
                        status = HostStatus.Operating;
                        inter.SendTo(hostInfo, BuildMessage(MessageIdentifier.ConfirmEncryption, null));
                        encryption = encrypt;
                        inter.Controller.OnHostAdded(hostInfo);
                    }
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
                byte[] finalData;
                if(status == HostStatus.Operating)
                {
                    finalData = encryption.Encrypt(data);
                }else
                {
                    finalData = data;
                }
                combinedMessage = new byte[finalData.Length+1];
                finalData.CopyTo(combinedMessage, 1);
            }
            combinedMessage[0] = (byte) message;
            return combinedMessage;
        }

    }
}
