using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Interfaces;

namespace MMOController
{
    /// <summary>
    /// A RMI interface for a node.
    /// </summary>
    public interface ILoginNode : IRMIInterface
    {

        /// <summary>
        /// Check a login to see if it works.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool Login(string username, string password);
    }
}
