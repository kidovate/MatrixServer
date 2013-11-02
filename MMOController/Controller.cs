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
            log.Info("Launching login node on new host...");
            var proxy = matrixPortal.GetNodeProxy<ILoginNode>(matrixPortal.LaunchNode<ILoginNode>());
		    log.Debug("It seems to have worked, result of Login(): "+proxy.Login("test", "test"));
		}
		#endregion
	}
}

