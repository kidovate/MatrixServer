using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MatrixHost.MasterInterface;
using log4net;

namespace MatrixHost.Nodes
{
    /// <summary>
    /// Downloads nodes from the library server.
    /// </summary>
    public class NodeLibDownloader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeLibDownloader));
        private HostClient clientInterface;
        public NodeLibDownloader(HostClient clientInterface)
        {
            this.clientInterface = clientInterface;
        }

        /// <summary>
        /// Download a library to the destination folder.
        /// </summary>
        /// <param name="library"></param>
        /// <param name="destFolder"></param>
        public void DownloadFile(string library, string destFolder)
        {
            log.Debug("Requesting library download URL for " + library + ".");
            var libUrl = clientInterface.RequestLibraryURL(library, int.MaxValue, this);
            using (WebClient Client = new WebClient ())
            {
                log.Debug("Downloading from "+libUrl+"...");
                Client.DownloadFile(libUrl, destFolder+"/"+library);
            }
        }
    }
}
