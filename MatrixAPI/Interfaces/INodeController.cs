using System;
using MatrixAPI.Data;
namespace MatrixAPI.Interfaces
{
	/// <summary>
	/// The Node Controller controls instancing of nodes in the system.
	/// </summary>
	public interface INodeController
	{
		/// <summary>
		/// Initialize the node controller.
		/// </summary>
		/// <param name='portal'>
		/// Matrix portal to access system resources
		/// </param>
		void Initialize(IControllerPortal portal);
		
		/// <summary>
		/// Called when a new host is added to the cluster.
		/// </summary>
		/// <param name='newHost'>
		/// New host identifier.
		/// </param>
		void OnHostAdded(HostInfo newHost);
		
	}
}

