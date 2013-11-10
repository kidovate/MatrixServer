using System;
using MMOController.Interfaces;
using MatrixAPI.Interfaces;
using MatrixAPI;
using MatrixAPI.Data;
using log4net;

namespace MMOController
{
	/// <summary>
	/// MMO cluster node controller
	/// </summary>
	public class MMOCluster : INodeController, IMMOCluster
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
		    matrixPortal.LaunchNode<ILoginNode>(newHost);
		    matrixPortal.LaunchNode<ILauncherNode>(newHost);

		    var proxy = matrixPortal.GetNodeProxy<ILoginNode>(matrixPortal.LaunchNode<ILoginNode>(newHost));

		    proxy.TestMethod("testing");
		}
		#endregion
	}
}

