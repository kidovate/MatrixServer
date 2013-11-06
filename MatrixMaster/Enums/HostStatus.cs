using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatrixMaster.Enums
{
    /// <summary>
    /// Host's status.
    /// </summary>
    public enum HostStatus : int
    {
        /// <summary>
        /// Host identity exchange.
        /// </summary>
        NoIdentity = 1,

        /// <summary>
        /// Host encryption exchange.
        /// </summary>
        NoEncryption = 2,

        /// <summary>
        /// Gone, dispose it now.
        /// </summary>
        Disconnected = 3,
		
		/// <summary>
		/// Working w/ node manager
		/// </summary>
		SyncNodes = 4,

        /// <summary>
        /// Waiting for ready message
        /// </summary>
        LoadingNodes = 5,
		
        /// <summary>
        /// Ready to launch Nodes.
        /// </summary>
        Operating = 5
    }
}
