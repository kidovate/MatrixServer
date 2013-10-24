using System;
using MatrixAPI.Interfaces;

namespace MMOController
{
	/// <summary>
	///  Initial and constant connection point for clients. Maintains login state of a client.
	/// </summary>
	public class LoginNode : INode
	{
		
		public LoginNode ()
		{
			Console.WriteLine ("New login node starting up...");
		}
		
		public void Initialize(IMatrixPortal portal){
			Console.WriteLine ("Initializing login node..");
		}
		
		public void Shutdown(){
			Console.WriteLine ("Login node shutting down..");
		
		}
	}
}

