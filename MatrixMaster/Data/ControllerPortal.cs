using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Data;
using MatrixAPI.Exceptions;
using MatrixAPI.Interfaces;
using MatrixMaster.Exceptions;
using MatrixMaster.Nodes;
using log4net;

namespace MatrixMaster.Data
{
    /// <summary>
    /// Controller portal implementation.
    /// </summary>
    public class ControllerPortal  : MatrixPortal, IControllerPortal
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ControllerPortal));
        /// <summary>
        /// Request a node launch in the cluster.
        /// </summary>
        /// <returns>
        /// Node info for launched node.
        /// </returns>
        /// <param name='nodeType'>
        /// Node type to launch in the cluster.
        /// </param>
        public NodeInfo LaunchNode<T>()
        {
            log.Debug("Launching a new node with type "+typeof(T).FullName);

            return NodePool.Instance.LaunchNode<T>();
        }

        /// <summary>
        /// Launch a new node in the cluster.
        /// </summary>
        /// <typeparam name="T">RMI type of node to launch.</typeparam>
        /// <param name="host">Host to launch the node on.</param>
        /// <returns></returns>
        public NodeInfo LaunchNode<T>(HostInfo host)
        {
            log.Debug("Launching a new node with type "+typeof(T).FullName);

            return NodePool.Instance.LaunchNode<T>(host);
        }

        /// <summary>
        /// Shuts down a node identified by NodeInfo
        /// </summary>
        /// <returns>
        /// Successful shutdown command?
        /// </returns>
        /// <param name='node'>
        /// Identifier for the node.
        /// </param>
        public void ShutdownNode(NodeInfo node)
        {
            NodePool.Instance.ShutdownNode(node);
        }

        /// <summary>
        /// Gets the Nodes running on a host.
        /// </summary>
        /// <returns>
        /// Nodes running on specified host.
        /// </returns>
        /// <param name='host'>
        /// Identifier for host
        /// </param>
        public NodeInfo[] GetHostNodes(HostInfo host)
        {
            return HostCache.FindHost(host.Id).Nodes.ToArray();
        }

        internal void SetNodeID(int id)
        {
            base.SetNodeID(id);
        }
    }
}
