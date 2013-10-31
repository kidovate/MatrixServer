using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MatrixMaster.Properties;
using log4net;

namespace MatrixMaster.Nodes
{
    /// <summary>
    /// Maintains node libraries on a HTTP accessable url for downloading.
    /// </summary>
    public class NodeHost
    {
        public static NodeHost Instance;
        private string libDirectory;
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeHost));
        /// <summary>
        /// Registers the http paths for node downloads and other.
        /// </summary>
        public NodeHost(string libDirectory)
        {
            Instance = this;
            this.libDirectory = libDirectory;
        }

        private Dictionary<string, string> registeredUris = new Dictionary<string, string>(); 

        /// <summary>
        /// Get the download URL for the library
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public string GetDownloadUrl(string library)
        {
            string newUri = (Path.GetRandomFileName().Replace(".", ""));
            registeredUris.Add(newUri, library);
            return Settings.Default.HTTPPort+"|"+newUri;
        }

        public byte[] GetDataForUri(string uri)
        {
            if (!registeredUris.ContainsKey(uri)) return null;
            
            var value = registeredUris[uri];
            
            registeredUris.Remove(uri);
            var data = File.ReadAllBytes(libDirectory + "/" + value);

            //log.Debug("File for requested uri ("+data.Length+"): " + libDirectory + "/" + value);
            return data;
        }
    }
}
