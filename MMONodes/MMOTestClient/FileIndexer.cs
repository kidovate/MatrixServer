using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OmuniOnlineLauncher
{
    /// <summary>
    /// Synchronizes and manages the node libraries (in the filesystem)
    /// </summary>
    public class FileIndexer
    {
        private string _gameFilePath;
        private Dictionary<string, byte[]> fsIndex;

        /// <summary>
        /// Indexed files as last updated by the indexer.
        /// </summary>
        public Dictionary<string, byte[]> FileIndex
        {
            get
            {
                if (fsIndex == null) Index();
                return fsIndex;
            }
        }

        /// <summary>
        /// Create a new node library manager.
        /// </summary>
        /// <param name="gameFilePath">Path to the libraries folder</param>
        public FileIndexer(string gameFilePath)
        {
            if (!Directory.Exists(gameFilePath)) Directory.CreateDirectory(gameFilePath);
            this._gameFilePath = gameFilePath;
        }

        /// <summary>
        /// Start up the manager. Does NOT perform the inital index.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Generate the map of the folder
        /// </summary>
        public void Index()
        {
            var files = Directory.GetFiles(_gameFilePath, "*", SearchOption.AllDirectories);
            var filesystemMap = new Dictionary<string, byte[]>();
            foreach (var file in files)
            {
                //Hash file
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        var hash = md5.ComputeHash(stream);
                        filesystemMap[file.Substring(_gameFilePath.Length + 1).Replace("\\", "/")] = hash;
                    }
                }
            }
            fsIndex = filesystemMap;
        }
    }
}
