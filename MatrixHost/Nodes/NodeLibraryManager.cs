using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MatrixAPI.Data;
using log4net;

namespace MatrixHost.Nodes
{
	/// <summary>
	/// Synchronizes and manages the node libraries (in the filesystem)
	/// </summary>
	public class NodeLibraryManager
	{
	    private string libraryPath;
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeLibraryManager));
	    private Dictionary<string, byte[]> fsIndex;
	    private NodeLibDownloader downloader;
        /// <summary>
        /// Indexed files as last updated by <see cref="NodeLibraryManager.IndexLibraries"/>.
        /// </summary>
	    public Dictionary<string, byte[]> FileIndex
	    {
	        get
	        {
                if(fsIndex == null) IndexLibraries();
	            return fsIndex;
	        }
	    }

        /// <summary>
        /// Create a new node library manager.
        /// </summary>
        /// <param name="libraryPath">Path to the libraries folder</param>
		public NodeLibraryManager (string libraryPath, NodeLibDownloader downloader)
        {
            this.downloader = downloader;
            if (!Directory.Exists(libraryPath)) Directory.CreateDirectory(libraryPath);
            this.libraryPath = libraryPath;
		}

        /// <summary>
        /// Start up the manager. Does NOT perform the inital index.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Generate the map of the library folder
        /// </summary>
	    public void IndexLibraries()
	    {
	        log.Debug("Filesystem map creation requested, loading files in path '"+libraryPath+"'...");
            var files = Directory.GetFiles(libraryPath, "*.dll");
            var filesystemMap = new Dictionary<string, byte[]>();
            foreach(var file in files)
	        {
	            //Hash file
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        var hash = md5.ComputeHash(stream);
                        filesystemMap[file] = hash;
                    }
                }
	        }
            log.Debug("Indexing complete with "+filesystemMap.Count+" entries.");
            fsIndex = filesystemMap;
	    }

        /// <summary>
        /// Perform some sync job on the filesystem. Downloads files using the NodeLibDownload.
        /// </summary>
        /// <param name="syncJob"></param>
	    public void PerformSyncJob(SyncJob syncJob)
	    {
	        log.Info("Performing filesystem sync, total of "+(syncJob.filesToDelete.Count+syncJob.filesToDownload.Count)+" operations.");
            log.Debug("Performing "+syncJob.filesToDelete.Count+" deletions.");
            foreach(var fileToDelete in syncJob.filesToDelete)
            {
                if(File.Exists(libraryPath+"/"+fileToDelete)) File.Delete(libraryPath+"/"+fileToDelete);
            }
            log.Debug("Performing "+syncJob.filesToDownload+" downloads.");
            foreach(var fileToDownload in syncJob.filesToDownload)
            {
                downloader.DownloadFile(fileToDownload, libraryPath);
            }
            log.Debug("Finished performing sync job.");
	    }
	}
}

