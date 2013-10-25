using ZeroMQ;

namespace MatrixMaster.Servers
{
    /// <summary>
    /// The server to deal with hosts.
    /// </summary>
    public class HostInterface
    {
        private ZmqContext context;
        private ZmqSocket server;
    }
}
