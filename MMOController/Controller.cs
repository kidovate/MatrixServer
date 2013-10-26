using System;
using MatrixAPI.Interfaces;
using MatrixAPI;
using MatrixAPI.Data;
using log4net;

namespace MMOController
{
	/// <summary>
	/// MMO cluster node controller
	/// </summary>
	public class MMOCluster : INodeController
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MMOCluster));
		IControllerPortal matrixPortal;
		public MMOCluster ()
		{
		}

		#region INodeController implementation
		public void Initialize (MatrixAPI.Interfaces.IControllerPortal portal)
		{
			matrixPortal = portal;
            log.Info("Node controller initialized.");
		}
	
		public void OnHostAdded (MatrixAPI.Data.HostInfo newHost)
		{
			Console.WriteLine("[Controller] Host added: "+newHost.Id);
			matrixPortal.LaunchNode<LoginNode>();
			
		}
		#endregion
	}
}

