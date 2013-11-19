using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Interfaces;

namespace MMOController
{
    public interface IMMOCluster : IRMIInterface
    {
		/// <summary>
		/// Determine if a given user is logged in.
		/// </summary>
		/// <returns><c>true</c> if this user is logged; otherwise, <c>false</c>.</returns>
		/// <param name="username">Username.</param>
		bool IsUserLoggedIn(string username);

		/// <summary>
		/// Register a user as logged in.
		/// </summary>
		/// <param name="username">Username.</param>
		void OnUserLoggedIn(string username);

		/// <summary>
		/// Register a user as gone.
		/// </summary>
		/// <param name="username">Username.</param>
		void OnUserLoggedOff(string username);
    }
}
