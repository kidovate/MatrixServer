using System;
using MatrixAPI.Interfaces;
using MatrixAPI.Data;
using System.Collections.Generic;

namespace MatrixAPI.Interfaces
{
	/// <summary>
	/// Expanded version of MatrixPortal with methods to launch or shutdown nodes.
	/// </summary>
	public interface IControllerPortal : IMatrixPortal
	{
		/// <summary>
		/// Request a node launch in the cluster.
		/// </summary>
		/// <returns>
		/// Node info for launched node.
		/// </returns>
		/// <param name='nodeType'>
		/// Node type to launch in the cluster.
		/// </param>
		NodeInfo LaunchNode<T>();

        /// <summary>
        /// Launch a new node in the cluster.
        /// </summary>
        /// <typeparam name="T">RMI type of node to launch.</typeparam>
        /// <param name="host">Host to launch the node on.</param>
        /// <returns></returns>
	    NodeInfo LaunchNode<T>(HostInfo host);
		
		/// <summary>
		/// Shuts down a node identified by NodeInfo
		/// </summary>
		/// <returns>
		/// Successful shutdown command?
		/// </returns>
		/// <param name='node'>
		/// Identifier for the node.
		/// </param>
		void ShutdownNode(NodeInfo node);
		
		/// <summary>
		/// Gets the nodes running on a host.
		/// </summary>
		/// <returns>
		/// Nodes running on specified host.
		/// </returns>
		/// <param name='host'>
		/// Identifier for host
		/// </param>
		NodeInfo[] GetHostNodes(HostInfo host);
	}
}

