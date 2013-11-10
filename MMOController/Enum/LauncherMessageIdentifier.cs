using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOController.Enum
{
    /// <summary>
    /// Message identifiers for the launcher 
    /// </summary>
    public enum LauncherMessageIdentifier : byte
    {
        /// <summary>
        /// Get a sync job with the md5 file index.
        /// </summary>
        GetSyncJob,
        Ping,
        UnknownMessage
    }
}
