﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using MMOController.Enum;
using MMOController.Interfaces;
using MMOController.Properties;
using MatrixAPI.Data;
using MatrixAPI.Interfaces;
using ProtoBuf;
using ZeroMQ;
using log4net;
using MatrixAPI.Util;

namespace MMOController.Nodes
{
    /// <summary>
    /// Hosts the information server for the launcher (synchronizes clients).
    /// </summary>
    public class LauncherNode : INode, ILauncherNode
    {
        private ZmqSocket server;
        private static readonly ILog log = LogManager.GetLogger(typeof(LauncherNode));
        private Dictionary<string, byte[]> fileIndex; 
        private Task serverTask;
        private int status = 0;

        /// <summary>
        /// Initialize the node
        /// </summary>
        /// <param name="portal">access portal to the Matrix. should be stored.</param>
        public void Initialize(IMatrixPortal portal)
        {
            log.Info("Launching a new launcher handler node.");

            log.Debug("Downloading file index from s3");
            using(MemoryStream ms = new MemoryStream())
            {
                MmoAws.AmazonS3.GetObject(new GetObjectRequest()
                                              {
                                                  BucketName = Settings.Default.BucketName,
                                                  Key = Settings.Default.FolderName + "/index.mhash"
                                              }).ResponseStream.CopyTo(ms);
                ms.Position = 0;
                fileIndex = Serializer.Deserialize<Dictionary<string, byte[]>>(ms);
            }

            log.Debug("Binding the server to port "+Settings.Default.LauncherInterfacePort);
			server = MmoZmq.context.CreateSocket(SocketType.REP);

            status = 1;
            serverTask = Task.Factory.StartNew(ServerThread);
        }

        void ServerThread()
        {
			server.Bind(Settings.Default.Protocol+"://*:" + Settings.Default.LauncherInterfacePort);
            while(status == 1)
            {
                var message = server.ReceiveMessage(TimeSpan.FromMilliseconds(500));
                if (message.FrameCount == 0) continue;

				log.Debug("Processing request: " + System.Enum.GetName(typeof(LauncherMessageIdentifier), message[0].Buffer[0]));

                switch((LauncherMessageIdentifier)message[0].Buffer[0])
                {
                    case LauncherMessageIdentifier.GetSyncJob:
                        SyncJob job = CreateJob(message[0].Buffer.Skip(1).ToArray());
                        byte[] data;
                        using(MemoryStream ms = new MemoryStream())
                        {
                            Serializer.Serialize(ms, job);
                            ms.Position = 0;
                            data = new byte[1+ms.Length];
                            data[0] = (byte) LauncherMessageIdentifier.GetSyncJob;
                            ms.ToArray().CopyTo(data, 1);
                        }
                        server.Send(data);
                        break;
                    case LauncherMessageIdentifier.Ping:
                        server.Send(message.First.Buffer);
                        break;
                    default:
                        server.Send(new byte[] {(byte) LauncherMessageIdentifier.UnknownMessage});
                        break;
                }

            }
			server.Unbind(Settings.Default.Protocol+"://*:"+Settings.Default.LauncherInterfacePort);
            log.Info("Launcher server shut down.");
        }

        private SyncJob CreateJob(byte[] toArray)
        {
            try
            {
                Dictionary<string, byte[]> index = toArray.Deserialize<Dictionary<string, byte[]>>();

                SyncJob result = new SyncJob();
                result.filesToDelete = new List<string>();
                result.filesToDownload = new List<string>();

                foreach (var file in index.Keys)
                {
                    if (!fileIndex.ContainsKey(file)) result.filesToDelete.Add(file);
                }

                foreach (var file in fileIndex)
                {
                    if (!index.ContainsKey(file.Key) || !index[file.Key].SequenceEqual(file.Value))
                    {
                        var downloadUrl =
                            MmoAws.AmazonS3.GetPreSignedURL(new GetPreSignedUrlRequest()
                                                                {
                                                                    BucketName = Settings.Default.BucketName,
                                                                    Key = Settings.Default.FolderName + "/" + file.Key,
                                                                    Expires = DateTime.Now.AddHours(3)
                                                                });
                        result.filesToDownload.Add(file.Key + "|" + downloadUrl);
                    }
                }

                return result;
            }catch(Exception ex)
            {
                log.Error("Error creating sync job, "+ex.Message);
                return new SyncJob();
            }
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
