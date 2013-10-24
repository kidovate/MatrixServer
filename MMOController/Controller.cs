using System;
using MatrixAPI.Interfaces;
using MatrixAPI;
using MatrixAPI.Data;

namespace MMOController
{
	/// <summary>
	/// MMO cluster node controller
	/// </summary>
	public class MMOCluster : INodeController
	{
		IControllerPortal matrixPortal;
		public MMOCluster ()
		{
		}

		#region INodeController implementation
		public void Initialize (MatrixAPI.Interfaces.IControllerPortal portal)
		{
			matrixPortal = portal;
		}
	
		public void OnHostAdded (MatrixAPI.Data.HostInfo newHost)
		{
			Console.WriteLine("[Controller] Host added: "+newHost.Id);
			matrixPortal.LaunchNode<LoginNode>();
			
		}
		#endregion
	}
}

