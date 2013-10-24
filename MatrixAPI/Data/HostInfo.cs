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
        public int Id;

        /// <summary>
        /// Host IP
        /// </summary>
        [ProtoMember(2)] 
		public string IPAddress;
    }
}
