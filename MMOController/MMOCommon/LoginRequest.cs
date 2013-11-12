using System;

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
		/// Sha1 hashed password, salted with client's unique salt.
		/// </summary>
		[ProtoMember(2)]
		public string Sha1Pass;
	}
}

