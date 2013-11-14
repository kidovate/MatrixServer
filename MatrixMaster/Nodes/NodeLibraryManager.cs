using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MatrixAPI.Data;
using log4net;

namespace MatrixMaster.Nodes
{
	/// <summary>
	/// Generates SyncJobs for the clients.
	/// </summary>
	public class NodeLibraryManager
	{
	    private string libraryPath;
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeLibraryManager));
	    private Dictionary<string, byte[]> fsIndex;

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
		public NodeLibraryManager (string libraryPath)
        {
            if (!Directory.Exists(libraryPath)) Directory.CreateDirectory(libraryPath);
            this.libraryPath = libraryPath;
            Instance = this;
        }

        /// <summary>
        /// Start up the manager, and perform the inital index
        /// </summary>
        public void Initialize()
        {
            IndexLibraries();
        }

        /// <summary>
        /// Generate the map of the library folder
        /// </summary>
	    public void IndexLibraries()
	    {
	        log.Debug("Filesystem map creation requested, loading files in path '"+libraryPath+"'...");
            var files = Directory.GetFiles(libraryPath, "*.*", SearchOption.AllDirectories).Where(file=>file.ToLower().EndsWith("dll") || file.ToLower().EndsWith("config")).Select(Path.GetFileName);
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
        /// Filename and Md5 index of the node libraries.
        /// </summary>
        /// <param name="inputIndex"></param>
	    public SyncJob CreateSyncJob(Dictionary<string, byte[]> inputIndex)
	    {
            //Search for files that do not exist
            var job = new SyncJob {filesToDelete = new List<string>(), filesToDownload = new List<string>()};

            foreach(var file in inputIndex.Keys)
            {
                if (!FileIndex.ContainsKey(file))
                {
                    job.filesToDelete.Add(file);
                }
            }

            foreach(var file in FileIndex.Keys)
            {
                if(!inputIndex.ContainsKey(file) || !inputIndex[file].SequenceEqual(FileIndex[file]))
                {
                    job.filesToDownload.Add(file);
                }
            }
            var operationCount = job.filesToDelete.Count + job.filesToDownload.Count;
            if(operationCount > 0) log.Debug("Generated SyncJob with "+(operationCount)+" operations.");
            return operationCount == 0 ? null : job;
	    }
	}
}

