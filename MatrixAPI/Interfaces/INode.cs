using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixAPI.Interfaces
{
    /// <summary>
    /// A node is an individual user code instance of functionality that is distributed over hosts.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Initialize the node
        /// </summary>
        /// <param name="portal">access portal to the Matrix. should be stored.</param>
        void Initialize(IMatrixPortal portal);

        /// <summary>
        /// End connections, release everything, and shut down.
        /// </summary>
        void Shutdown();
    }
}
