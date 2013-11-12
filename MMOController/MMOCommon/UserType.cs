using System;

namespace MMOCommon
{
	/// <summary>
	/// Different user types in the system.
	/// </summary>
	public enum UserType : byte
	{
		/// <summary>
		/// Full privileges admin
		/// </summary>
		Admin,

		/// <summary>
		/// A lesser privileges game master
		/// </summary>
		GameMaster,

		/// <summary>
		/// A normal player
		/// </summary>
		Player
	}
}

