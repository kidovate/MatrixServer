using System;

namespace MMOCommon
{
	/// <summary>
	/// Identifiers for messages between server <-> game
	/// </summary>
	public enum MessageIdentifier : byte
	{
		/// <summary>
		/// The initial "hello".
		/// </summary>
		Init,

		/// <summary>
		/// Server assigning client an identity.
		/// </summary>
		SetIdentity,

		/// <summary>
		/// Server tells client what the status of the system is. Could happen whenever.
		/// Statuses are single-byte defined in <see cref="ClusterStatus"/> 
		/// </summary>
		SetStatus,

		/// <summary>
		/// Server requesting encryption md5.
		/// </summary>
		BeginEncryption,

		/// <summary>
		/// Encryption key is not registered in the server. Client will be disconnected.
		/// </summary>
		InvalidKey,

		/// <summary>
		/// Confirm encryption and begin login sequence.
		/// Data is the password salt to use during login.
		/// </summary>
		ConfirmEncryption,

		/// <summary>
		/// Client login request, response will be LoginVerify with true or false.
		/// </summary>
		LoginVerify,

		/// <summary>
		/// Request / broadcast list of characters and associated data
		/// </summary>
		CharacterData,

		/// <summary>
		/// Select a character. Response with true or false, true -> server will process you in the system.
		/// If true, wait until the server begins telling you to join zones.
		/// </summary>
		CharacterVerify,

		/// <summary>
		/// Server tells client to connect to a zone. Data is <see cref="ZoneInfo"/>. 
		/// </summary>
		ZoneConnect,

		/// <summary>
		/// Server tells client to disconnect from a zone. Data is <see cref="ZoneInfo"/>
		/// </summary>
		ZoneDisconnect,

		/// <summary>
		/// Server tells client to start using the next zone as the primary.
		/// </summary>
		ZoneTransition,

		/// <summary>
		/// Disconnect the client.
		/// </summary>
		Disconnect,

		/// <summary>
		/// Simple heartbeat. Server expects a 10 second heartbeat.
		/// </summary>
		Heartbeat,
        
        /// <summary>
        /// Invalid identity
        /// </summary>
	    InvalidIdentity
	}
}

