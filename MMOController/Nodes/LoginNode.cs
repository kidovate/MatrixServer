using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using MMOController.Properties;
using MatrixAPI.Interfaces;
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
						var client = new Client();
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
			log.Info("Game handler shut down.");
		}

		private SyncJob CreateJob(byte[] toArray)
		{
			Dictionary<string, byte[]> index;
			using(MemoryStream ms = new MemoryStream())
			{
				ms.Write(toArray, 0, toArray.Length);
				ms.Position = 0;
				index = Serializer.Deserialize<Dictionary<string, byte[]>>(ms);
			}

			SyncJob result = new SyncJob();
			result.filesToDelete = new List<string>();
			result.filesToDownload = new List<string>();

			foreach(var file in index.Keys)
			{
				if(!fileIndex.ContainsKey(file)) result.filesToDelete.Add(file);
			}

			foreach(var file in fileIndex)
			{
				if(!index.ContainsKey(file.Key) || !index[file.Key].SequenceEqual(file.Value))
				{
					var downloadUrl =
						MmoAws.AmazonS3.GetPreSignedURL(new GetPreSignedUrlRequest()
							{
								BucketName = Settings.Default.BucketName,
								Key = Settings.Default.FolderName + "/" + file.Key,
								Expires = DateTime.Now.AddHours(3)
							});
					result.filesToDownload.Add(file.Key+"|"+downloadUrl);
				}
			}

			return result;
		}

		/// <summary>
		/// End connections, release everything, and shut down.
		/// </summary>
		public void Shutdown()
		{
			status = 0;
		}
	}
}

