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
		/// Shuts down a node identified by NodeInfo
		/// </summary>
		/// <returns>
		/// Successful shutdown command?
		/// </returns>
		/// <param name='node'>
		/// Identifier for the node.
		/// </param>
		bool ShutdownNode(NodeInfo node);
		
		/// <summary>
		/// Gets the nodes running on a host.
		/// </summary>
		/// <returns>
		/// Nodes running on specified host.
		/// </returns>
		/// <param name='host'>
		/// Identifier for host
		/// </param>
		List<NodeInfo> GetHostNodes(HostInfo host);
	}
}

