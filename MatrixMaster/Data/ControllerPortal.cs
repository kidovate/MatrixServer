using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Data;
using MatrixAPI.Interfaces;

namespace MatrixMaster.Data
{
    /// <summary>
    /// Controller portal implementation.
    /// </summary>
    public class ControllerPortal  : IControllerPortal
    {
        /// <summary>
        /// Retreive a proxied node RMI identified by ID.
        /// </summary>
        /// <typeparam name="T">RMI interface</typeparam>
        /// <param name="identifier">NodeInfo identifier object</param>
        /// <returns></returns>
        public T GetNodeProxy<T>(NodeInfo identifier)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve a proxied node RMI with lowest load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNodeProxy<T>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get node list of a type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rmi"></param>
        /// <returns></returns>
        public List<NodeInfo> GetNodeList<T>(T rmi)
        {
            throw new NotImplementedException();
        }

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
            Console.WriteLine("TEMP = Launch node requested");
            return null;
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
        public bool ShutdownNode(NodeInfo node)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the nodes running on a host.
        /// </summary>
        /// <returns>
        /// Nodes running on specified host.
        /// </returns>
        /// <param name='host'>
        /// Identifier for host
        /// </param>
        public List<NodeInfo> GetHostNodes(HostInfo host)
        {
            throw new NotImplementedException();
        }
    }
}
