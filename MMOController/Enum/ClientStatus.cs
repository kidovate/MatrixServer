using System;

namespace MMOController
{
	public enum ClientStatus
	{
		/// <summary>
		/// Client identity exchange.
		/// </summary>
		NoIdentity = 1,

		/// <summary>
		/// Client encryption exchange.
		/// </summary>
		NoEncryption = 2,

		/// <summary>
		/// Gone, dispose it now.
		/// </summary>
		Disconnected = 3,

		/// <summary>
		/// Logging in phase. 
		/// </summary>
		LoggingIn = 4,

        /// <summary>
        /// Selecting a character.
        /// </summary>
        CharacterSelect = 5,

		/// <summary>
		/// Loading the next level (before connecting to zone nodes)
		/// </summary>
		Loading = 6,

		/// <summary>
		/// Playing, currently on some zone servers
		/// </summary>
		Playing = 7
	}
}

