using System;
using ProtoBuf;

namespace MMOCommon
{
	/// <summary>
	/// Character information to be given to client.
	/// </summary>
	[ProtoContract]
	public class CharacterData
	{
		/// <summary>
		/// Id of the character.
		/// </summary>
		[ProtoMember(1)]
		public int Id;

		/// <summary>
		/// Name of the character
		/// </summary>
		[ProtoMember(2)]
		public string Name;

		/// <summary>
		/// Experience of the character
		/// </summary>
		[ProtoMember(3)]
		public int XP;

		/// <summary>
		/// True if male.
		/// </summary>
		[ProtoMember(4)]
		public bool Gender;
	}
}

