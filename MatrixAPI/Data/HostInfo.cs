using ProtoBuf;

namespace MatrixAPI.Data
{
    /// <summary>
    /// Identification info for a host
    /// </summary>
    [ProtoContract]
    public class HostInfo
    {
        /// <summary>
        /// Host ID
        /// </summary>
        [ProtoMember(1)]
        public byte[] Id;
    }
}
