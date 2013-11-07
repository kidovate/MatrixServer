using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using MMOController.Properties;
using MatrixAPI.Interfaces;
using log4net;

namespace MMOController
{
	/// <summary>
	///  Initial and constant connection point for clients (game engine). Maintains login state of a client.
	/// </summary>
	public class LoginNode : INode, ILoginNode
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

