using System;
using ProtoBuf;

namespace MMOCommon
{
	/// <summary>
	/// Basic information about a zone.
	/// </summary>
	[ProtoContract]
	public class ZoneInfo
	{
		/// <summary>
		/// Identifier
		/// </summary>
		[ProtoMember(1)]
		public int Id;

		/// <summary>
		/// Zone name to be displayed to the users upon entering
		/// </summary>
		[ProtoMember(2)]
		public string Name;

		/// <summary>
		/// Temporary info: IP of the node hosting the zone.
		/// </summary>
		[ProtoMember(3)]
		public string NodeIP;

		/// <summary>
		/// Temporary info: port of the node hosting the zone.
		/// </summary>
		[ProtoMember(4)]
		public int NodePort;

		/// <summary>
		/// The boundary points of the zone.
		/// </summary>
		[ProtoMember(5)]
		public Vector2D[] Boundary;
	}
}

