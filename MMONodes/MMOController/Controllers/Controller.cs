using System;
using MMOController.Interfaces;
using MatrixAPI.Interfaces;
using MatrixAPI;
using MatrixAPI.Data;
using log4net;
using System.Collections.Generic;

namespace MMOController
{
	/// <summary>
	/// MMO cluster node controller
	/// </summary>
	public class MMOCluster : INodeController, IMMOCluster
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MMOCluster));
		private List<string> users = new List<string>();
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
		}

		/// <summary>
		/// Is the user logged in?
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="username">Username.</param>
		public bool IsUserLoggedIn(string username){
			//Check if the user is logged in
			return users.Contains(username.ToLower());
		}

		/// <summary>
		/// Register a user as logged in.
		/// </summary>
		/// <param name="username">Username.</param>
		public void OnUserLoggedIn(string username){
			if(!users.Contains(username.ToLower())) users.Add(username.ToLower());
		}

		/// <summary>
		/// When a user logs off.
		/// </summary>
		/// <param name="username">Username.</param>
		public void OnUserLoggedOff(string username){
			users.Remove(username.ToLower());
		}

		#endregion
	}
}

