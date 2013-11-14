using System;
using ProtoBuf;

namespace MMOController
{
	/// <summary>
	/// Identification info for a host
	/// </summary>
	[ProtoContract]
	public class ClientInfo
	{
		/// <summary>
		/// Client ID
		/// </summary>
		[ProtoMember(1)]
		public byte[] Id;
	}
}

