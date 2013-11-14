using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using MatrixAPI.Data;
using MatrixHost.MasterInterface;
using MatrixHost.Properties;
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
	    private HostClient clientInterface;
	    public static NodeLibraryManager Instance;
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
		public NodeLibraryManager (string libraryPath, HostClient clientInterface)
        {
            this.clientInterface = clientInterface;
            if (!Directory.Exists(libraryPath)) Directory.CreateDirectory(libraryPath);
            this.libraryPath = libraryPath;
            Instance = this;
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
            var files = Directory.GetFiles(libraryPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var filesystemMap = new Dictionary<string, byte[]>();
            foreach(var file in files)
	        {
	            //Hash file
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(libraryPath+"/"+file))
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
            if(syncJob.filesToDelete== null) syncJob.filesToDelete = new List<string>(0);
            if(syncJob.filesToDownload ==null) syncJob.filesToDownload = new List<string>(0);
	        log.Info("Performing filesystem sync, total of "+(syncJob.filesToDelete.Count+syncJob.filesToDownload.Count)+" operations.");
            log.Debug("Performing "+syncJob.filesToDelete.Count+" deletions.");
            foreach(var fileToDelete in syncJob.filesToDelete)
            {
                if(File.Exists(libraryPath+"/"+fileToDelete)) File.Delete(libraryPath+"/"+fileToDelete);
            }
            log.Debug("Performing "+syncJob.filesToDownload.Count+" downloads.");
            foreach(var fileToDownload in syncJob.filesToDownload)
            {
                DownloadFile(fileToDownload, libraryPath);
            }
            log.Debug("Finished performing sync job.");
	    }

        /// <summary>
        /// Download a library to the destination folder.
        /// </summary>
        /// <param name="library"></param>
        /// <param name="destFolder"></param>
        public void DownloadFile(string library, string destFolder)
        {
            //log.Debug("Requesting library download URL for " + library + ".");
            var respose = clientInterface.RequestLibraryURL(library, int.MaxValue, this).Split('|');
            var libUrl = "http://" + Settings.Default.MasterIP + ":" + respose[0] + "/" + respose[1];
            using (WebClient Client = new WebClient())
            {
                log.Debug("Downloading from " + libUrl + "...");
                Client.DownloadFile(libUrl, destFolder + "/" + library);
            }
        }
	}
}

