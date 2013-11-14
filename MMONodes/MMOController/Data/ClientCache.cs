using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOController.Data
{
    /// <summary>
    /// Stores the connected clients.
    /// </summary>
    public class ClientCache
    {
        private static Dictionary<byte[], Client> clients = new Dictionary<byte[], Client>();

        /// <summary>
        /// All of the clients currently connected.
        /// </summary>
        public static Dictionary<byte[], Client> ConnectedClients
        {
            get { return clients; }
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
        public static void RegisterClient(Client host)
        {
            clients[host.Id] = host;
        }

        /// <summary>
        /// Find a client by identity
        /// </summary>
        /// <param name="identity"></param>
        public static Client FindClient(byte[] identity)
        {
            return clients.Keys.Any(a => a.SequenceEqual(identity)) ? clients.FirstOrDefault(e => e.Key.SequenceEqual(identity)).Value : null;
        }
    }
}
