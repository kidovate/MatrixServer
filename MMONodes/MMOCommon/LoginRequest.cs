using System;
using ProtoBuf;

namespace MMOCommon
{
	/// <summary>
	/// A login request
	/// </summary>
	[ProtoContract]
	public class LoginRequest
	{
		[ProtoMember(1)]
		public string Username;

		/// <summary>
		/// Raw password is hashed with salt before it, and then hashed again with salt after it.
		/// </summary>
		[ProtoMember(2)]
		public string MD5Pass;
	}

    /// <summary>
    /// A login response
    /// </summary>
    [ProtoContract]
    public class LoginResponse
    {
        /// <summary>
        /// If successful proceed to requesting character info
        /// </summary>
        [ProtoMember(1)]
        public bool Success;

        /// <summary>
        /// Message if failed. Blank or null if success.
        /// </summary>
        [ProtoMember(2)]
        public string Message;
    }
}

