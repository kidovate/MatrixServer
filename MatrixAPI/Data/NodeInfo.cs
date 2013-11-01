using System.Linq;
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
        [ProtoMember(3)] public byte[] HostID;

        /// <summary>
        /// Fully qualified RMI interface for node
        /// </summary>
        [ProtoMember(2)] public string RMITypeName;

        /// <summary>
        /// Is it exactly the same as another?
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (!(other is NodeInfo)) return false;
            var otherNode = (NodeInfo) other;
            return otherNode.Id == Id && otherNode.RMITypeName.Equals(RMITypeName) &&
                   otherNode.HostID.SequenceEqual(otherNode.HostID);
        }
    }
}
