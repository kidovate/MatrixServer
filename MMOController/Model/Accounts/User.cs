using System;

namespace MMOController.Model.Accounts
{
	/// <summary>
	/// The main database model for a user in the system.
	/// </summary>
	public class User
	{
		/// <summary>
		/// The ID of the user.
		/// </summary>
		/// <value>The identifier.</value>
		public virtual int Id { get; protected set; }

		/// <summary>
		/// Plaintext all lowercase username.
		/// </summary>
		/// <value>The username.</value>
		public virtual string Username {get;set;}

		/// <summary>
		/// Actually an unsalted password. 
		/// </summary>
		/// <value>Unsalted password.</value>
		public virtual string Password {get;set;}

		/// <summary>
		/// Gets the user type of the user.
		/// </summary>
		/// <value>The role.</value>
		public virtual UserType Role {get;set;}
	}
}

