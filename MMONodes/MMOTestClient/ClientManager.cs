using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MMOCommon;
using MMOTestClient.Properties;
using MatrixAPI.Encryption;
using MatrixAPI.Util;
using OmuniOnlineLauncher;
using ProtoBuf;
using ZeroMQ;

namespace MMOTestClient
{
    public class ClientManager
    {
        private MainWindow window;
        private bool started = false;
        private CancellationTokenSource cancel;

        public ClientManager(MainWindow window)
        {
            this.window = window;
            heartbeat.Elapsed += (sender, args) => SendHeartbeat();
        }

        public void Start()
        {
            if (started) return;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead("EncryptionKey.mek"))
                {
                    keyHash = md5.ComputeHash(stream);
                    encryption = new AES(Encoding.UTF8, "EncryptionKey.mek");
                }
            }
            Task.Factory.StartNew(ClientThread);
            //ClientThread();
        }

        public void ForceQuit()
        {
            cancel.Cancel();
        }

        private ZmqContext context;
        private ZmqSocket socket;

        private Thread socketThread;

        private byte[] assignedIdentity;

        private int status = 0;

        private System.Timers.Timer heartbeat = new System.Timers.Timer(15000);

        private AES encryption;

        private byte[] keyHash;

        private int heartbeatAttempts = 0;

        /// <summary>
        /// Conditionally send out a heartbeat
        /// </summary>
        private void SendHeartbeat()
        {
            //if (heartbeatAttempts > 4)
            //{
            //    window.Log("Heartbeat timeout for " + heartbeat.Interval * heartbeatAttempts / 1000 + " seconds...");
              //  heartbeatAttempts++;
              //  return;
            //}

            socket.Send(BuildMessage(MessageIdentifier.Heartbeat, null, true));
            heartbeatAttempts++;
        }

        private void ClientThread()
        {
            try
            {

                status = 1;
                context = ZmqContext.Create();
                socket = context.CreateSocket(SocketType.DEALER);
                window.SetProgress(5);
                window.SetStatus("Contacting server...");
                window.Log("Attempting connection to server...");
                window.Log("   --> " + Settings.Default.ServerAddress);
                bool connected = false;
                {
                    byte[] identity = new byte[15];
                    new Random().NextBytes(identity);
                    socket.Identity = identity;
                    var helloMessage = new[] { (byte)MessageIdentifier.Init };
                    int attempt = 1;
                    var endpoint = Settings.Default.ServerAddress;
                    socket.Connect(endpoint);
                    socket.Send(helloMessage);
                    while (!connected)
                    {
                        var message = socket.ReceiveMessage(TimeSpan.FromSeconds(5));
                        if (message.TotalSize == 0)
                        {
                            window.Log("(" + attempt + ") Failed to connect, trying again in 5 seconds...");
                        }
                        else
                        {
                            window.Log("Received response, verifying...");

                            //check command value
                            if (message.First.Buffer[0] == (byte)MessageIdentifier.SetIdentity)
                            {
                                assignedIdentity = message.First.Buffer.Skip(1).ToArray();
                                window.Log("Received a new " + assignedIdentity.Length + " byte identity.");
                                socket.Identity = assignedIdentity;
                                socket.Disconnect(endpoint);
                                var finalSocket = context.CreateSocket(SocketType.DEALER);
                                socket.Dispose();
                                socket = finalSocket;
                                socket.Identity = assignedIdentity;
                                socket.TcpKeepalive = TcpKeepaliveBehaviour.Enable;
                                socket.Connect(endpoint);
                                socket.Send(new byte[] { (byte)MessageIdentifier.SetIdentity });
                                break;
                            }
                            if (message.First.Buffer[0] == (byte)MessageIdentifier.InvalidIdentity)
                            {
                                window.Log("Server responded with invalid identity. Trying again.");
                            }
                        }
                        attempt++;
                        Thread.Sleep(5000);
                    }
                    window.Log("Connected to master server after " + attempt + " attempts.");
                    window.SetProgress(20);
                    window.SetStatus("Synchronizing encryption...");

                }
                //We are connected!
                window.Log("Waiting for encryption sequence...");
                ZmqMessage msg = socket.ReceiveMessage();
                if (msg.First.Buffer[0] != (byte)MessageIdentifier.BeginEncryption)
                {
                    LogUnexpectedMessage(msg.First.Buffer);
                    window.Log("Crucial unexpected result, disconnecting.");
                    socket.Send(new byte[] { (byte)MessageIdentifier.Disconnect });
                    status = 0;
                    socket.Dispose();
                    context.Dispose();
                    return;
                }

                window.Log("Sending encryption key...");
                byte[] encryptMsg = new byte[1 + keyHash.Length];
                encryptMsg[0] = (byte)MessageIdentifier.BeginEncryption;
                keyHash.CopyTo(encryptMsg, 1);
                socket.Send(encryptMsg);

                window.Log("Waiting for response...");

                msg = socket.ReceiveMessage();

                if (msg.First.Buffer[0] != (byte)MessageIdentifier.ConfirmEncryption)
                {
                    window.Log("Invalid encryption key. Exiting...");
                    status = 0;
                    socket.Send(BuildMessage(MessageIdentifier.Disconnect, null, false));
                    return;
                }

                string salt = Encoding.UTF8.GetString(DecryptMessage(msg.First.Buffer.Skip(1).ToArray()));

                heartbeat.Start();

                window.Log("Connected, encrypted, beginning login.");
                window.SetStatus("Logging in...");
                window.SetProgress(70);

                bool loggedIn = false;
                while (!loggedIn)
                {
                    LoginRequest req = LoginForm.CreateRequest(salt);
                    var serialized = req.Serialize();
                    socket.Send(BuildMessage(MessageIdentifier.LoginVerify, serialized, true));
                    msg = socket.ReceiveMessage();
                    var data = DecryptMessage(msg.First.Buffer.Skip(1).ToArray());
                    LoginResponse resp = data.Deserialize<LoginResponse>();
                    if (resp.Success) loggedIn = true;
                    else
                    {
                        MessageBox.Show("Failed login, message: " + resp.Message);
                    }

                    window.SetProgress(100);
                    window.SetStatus("Logged in!");
                    window.Log("Received response from server, we are logged in!");
                }

                socket.Send(BuildMessage(MessageIdentifier.Disconnect, null, true));
                socket.Disconnect(Settings.Default.ServerAddress);

            }
            catch (Exception ex)
            {
                window.Log("Error: " + ex.Message);
                window.SetStatus("Error, see log.");
            }
        }

        /// <summary>
        /// Decrypt an encrypted message.
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        private byte[] DecryptMessage(byte[] encrypted)
        {
            return encryption.Decrypt(encrypted);
        }

        private void LogUnexpectedMessage(byte[] buffer)
        {
            window.Log("Unexpected message: " + Enum.GetName(typeof(MessageIdentifier), buffer[0]));
        }

        /// <summary>
        /// Build a message based on an identifier and a data array.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] BuildMessage(MessageIdentifier message, byte[] data, bool encryp)
        {
            byte[] combinedMessage;
            if (data == null)
                combinedMessage = new byte[1];
            else
            {
                //Encrypt the data if needed
                byte[] finalData = encryp ? encryption.Encrypt(data) : data;
                combinedMessage = new byte[finalData.Length + 1];
                finalData.CopyTo(combinedMessage, 1);
            }
            combinedMessage[0] = (byte)message;
            return combinedMessage;
        }
    }
}