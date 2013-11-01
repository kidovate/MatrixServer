using System;
using System.Collections.Generic;
using System.Linq;
using MatrixAPI.Data;

namespace MatrixMaster.Nodes
{
    /// <summary>
    /// The system's node pool contains all of the nodes and their identifying information.
    /// </summary>
    public class NodePool
    {
        Dictionary<int, NodeInfo> nodes = new Dictionary<int, NodeInfo>();
        public static NodePool Instance;

        public NodePool()
        {
            Instance = this;
        }

        /// <summary>
        /// Register a new node in the system.
        /// </summary>
        /// <param name="info"></param>
        public void RegisterNode(NodeInfo info)
        {
            if (nodes.ContainsKey(info.Id)) throw new IndexOutOfRangeException("node already exists");

            nodes.Add(info.Id, info);
        }

        /// <summary>
        /// Internal use - destroys a node from the index assuming it has already been cleaned up.
        /// </summary>
        /// <param name="info"></param>
        public void DestroyNode(NodeInfo info)
        {
            nodes.Remove(info.Id);
        }

        /// <summary>
        /// Retrieve a node's information based on its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NodeInfo NodeForId(int id)
        {
            if (!nodes.ContainsKey(id)) return null;
            return nodes[id];
        }

        /// <summary>
        /// Return a node running an RMI type.
        /// </summary>
        /// <param name="rmiType"></param>
        /// <returns></returns>
        public NodeInfo NodeForRMI(Type rmiType)
        {
            return nodes.Values.FirstOrDefault(e => e.RMITypeName == rmiType.FullName);
        }

        /// <summary>
        /// Check if the specified NodeInfo is in the pool.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool CheckNodeExists(NodeInfo info)
        {
            var node = nodes.Values.FirstOrDefault(e => e.Equals(info));
            return node != null;
        }
    }
}
