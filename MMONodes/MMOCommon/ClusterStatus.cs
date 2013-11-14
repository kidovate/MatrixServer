using System;

namespace MMOCommon
{
	/// <summary>
	/// Defines the status of the system
	/// </summary>
	public enum ClusterStatus : byte
	{
		/// <summary>
		/// Rejecting any further connections
		/// </summary>
		NoConnections,
		/// <summary>
		/// System is running normally
		/// </summary>
		Running,
		/// <summary>
		/// System is shutting down, notify users.
		/// </summary>
		ShuttingDown
	}
}

