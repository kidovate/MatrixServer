using System.Collections.Generic;
using MatrixAPI.Data;

namespace MatrixAPI.Interfaces
{

    /// <summary>
    /// Gives nodes access to the network, and to other nodes over the network
    /// </summary>
    public interface IMatrixPortal
    {
        /// <summary>
        /// Retreive a proxied node RMI identified by ID.
        /// </summary>
        /// <typeparam name="T">RMI interface</typeparam>
        /// <param name="identifier">NodeInfo identifier object</param>
        /// <returns></returns>
        T GetNodeProxy<T>(NodeInfo identifier);

        /// <summary>
        /// Retrieve a proxied node RMI with lowest load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetNodeProxy<T>();

        /// <summary>
        /// Get node list of a type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rmi"></param>
        /// <returns></returns>
        List<NodeInfo> GetNodeList<T>(T rmi); 
    }
}
