using System;

namespace MatrixHost
{
	/// <summary>
	/// Synchronizes and manages the node libraries.
	/// </summary>
	public class NodeLibraryManager
	{
		HostInterface hostInter;
		
		
		public NodeLibraryManager (HostInterface hostInter)
		{
			this.hostInter = hostInter;
		}
	}
}

