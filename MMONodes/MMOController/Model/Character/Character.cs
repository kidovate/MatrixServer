using System;
using MMOCommon;
using MMOController.Model.Accounts;

namespace MMOController.Model.Character
{
	/// <summary>
	/// The main database model for a character in the system.
	/// </summary>
	public class Character 
	{
		/// <summary>
		/// The ID of the character.
		/// </summary>
		/// <value>The identifier.</value>
		public virtual int Id { get; protected set; }

		/// <summary>
		/// Human Readable name of the character.
		/// </summary>
		/// <value>The full name.</value>
		public virtual string Name {get;set;}

		/// <summary>
		/// Gender of the character. True is male.
		/// </summary>
		/// <value>Gender.</value>
		public virtual bool Gender {get;set;}

		/// <summary>
		/// XP of the character.
		/// </summary>
		/// <value>Character XP.</value>
		public virtual int XP {get;set;}

		/// <summary>
		/// The user that owns this character
		/// </summary>
		/// <value>The user.</value>
		public virtual User User {get;set;}
	}
}

