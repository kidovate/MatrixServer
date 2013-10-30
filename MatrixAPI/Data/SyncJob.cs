using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace MatrixAPI.Data
{
    /// <summary>
    /// A sync job - server wants client to do the following with the nodes.
    /// </summary>
    [ProtoContract]
    public class SyncJob
    {
        /// <summary>
        /// Files to delete
        /// </summary>
        [ProtoMember(1)]
        public List<string> filesToDelete;

        /// <summary>
        /// Out of date files or files to re-download.
        /// </summary>
        [ProtoMember(2)]
        public List<string> filesToDownload;
    }
}
