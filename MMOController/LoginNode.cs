using System;
using System.Threading.Tasks;
using MatrixAPI.Interfaces;
using log4net;

namespace MMOController
{
	/// <summary>
	///  Initial and constant connection point for clients. Maintains login state of a client.
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
		    Task.Factory.StartNew(() =>
		                              {
		                                  log.Debug("Attempting test RMI");
		                                  try
		                                  {
		                                      var proxy = portal.GetNodeProxy<IMMOCluster>();
		                                      log.Debug("Response: " + proxy.TestString(5));
		                                  }
		                                  catch (Exception ex)
		                                  {
		                                      log.Error("Exception while testing RMI: " + ex.Message);
		                                  }
		                              });
		}
		
		public void Shutdown(){
			log.Info("Login node shutting down..");
		}

	    /// <summary>
	    /// Check a login to see if it works.
	    /// </summary>
	    /// <param name="username"></param>
	    /// <param name="password"></param>
	    /// <returns></returns>
	    public bool Login(string username, string password)
	    {
	        return true;
	    }
	}
}

