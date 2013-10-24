using ProtoBuf;

namespace MatrixAPI.Data
{
    /// <summary>
    /// Data transport of basic node information.
    /// </summary>
    [ProtoContract]
    public class NodeInfo
    {
        /// <summary>
        /// Node ID
        /// </summary>
        [ProtoMember(1)]
        public int Id;

        /// <summary>
        /// ID of host staging node
        /// </summary>
        [ProtoMember(3)] public int HostID;

        /// <summary>
        /// Fully qualified RMI interface for node
        /// </summary>
        [ProtoMember(2)] public string QualifiedRMI;
    }
}
