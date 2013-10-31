using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatrixMaster.Nodes
{
    /// <summary>
    /// Maintains node libraries on a HTTP accessable url for downloading.
    /// </summary>
    public class NodeHost
    {
        //I guess for now I'll just have one url for one lib for testing.
        //This needs to be properly implemented later
        public static NodeHost Instance;

        /// <summary>
        /// A host from which hosts can download library files
        /// </summary>
        public NodeHost()
        {
            Instance = this;
        }

        /// <summary>
        /// Get the download URL for the library
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public string GetDownloadUrl(string library)
        {
            return "dl.dropboxusercontent.com/u/1330616/Documentation-2013-07-22.zip";
        }
    }
}
