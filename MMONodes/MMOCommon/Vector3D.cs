using System;
using ProtoBuf;

namespace MMOCommon
{
	/// <summary>
	/// A 2D vector, for representing an X and Y position.
	/// </summary>
	[ProtoContract]
	public class Vector3D
	{
		/// <summary>
		/// The X position
		/// </summary>
		[ProtoMember(1)]
		public double XPosition;

		/// <summary>
		/// The Y position.
		/// </summary>
		[ProtoMember(2)]
		public double YPosition;

		/// <summary>
		/// The Z position.
		/// </summary>
		[ProtoMember(3)]
		public double ZPosition;
	}
}

