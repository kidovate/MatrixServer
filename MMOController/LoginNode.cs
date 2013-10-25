using System;
using MatrixAPI.Interfaces;
using log4net;

namespace MMOController
{
	/// <summary>
	///  Initial and constant connection point for clients. Maintains login state of a client.
	/// </summary>
	public class LoginNode : INode
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginNode));
		public LoginNode ()
		{
			log.Info("New login node created.");
		}
		
		public void Initialize(IMatrixPortal portal){
			log.Debug("Initializing login node..");

            log.Debug("Login node initialized.");
		}
		
		public void Shutdown(){
			log.Info("Login node shutting down..");
		
		}
	}
}

