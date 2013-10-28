using System;
using System.Collections.Generic;
using System.Linq;
using ZeroMQ;

namespace MatrixMaster.Data
{
    /// <summary>
    /// Maintains a list of all the hosts.
    /// </summary>
    public class HostCache
    {
        private static Dictionary<byte[], Host> hosts = new Dictionary<byte[], Host>();
        private static Dictionary<byte[], Host> deadHosts = new Dictionary<byte[], Host>();
        
        /// <summary>
        /// All of the hosts currently connected.
        /// </summary>
        public static Dictionary<byte[], Host> ConnectedHosts
        {
            get { return hosts; }
        }

        /// <summary>
        /// Suspended / disconnected / dead hosts.
        /// </summary>
        public static Dictionary<byte[], Host> DeadHosts
        {
            get { return deadHosts; }
        }

        /// <summary>
        /// Return a new random ID.
        /// </summary>
        /// <returns></returns>
        public static byte[] RandomId()
        {
            var rnd = new Random();
            var id = new byte[27];
            rnd.NextBytes(id);
            return id;
        }

        /// <summary>
        /// Register a new host in the database.
        /// </summary>
        /// <param name="host"></param>
        public static void RegisterHost(Host host)
        {
            hosts[host.Id] = host;
        }

        /// <summary>
        /// Find a host by identity
        /// </summary>
        /// <param name="identity"></param>
        public static Host FindHost(byte[] identity)
        {
            return hosts.Keys.Any(a => a.SequenceEqual(identity)) ? hosts.FirstOrDefault(e => e.Key.SequenceEqual(identity)).Value : null;
        }
    }
}
