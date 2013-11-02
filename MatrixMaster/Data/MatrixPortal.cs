using System;
using System.Collections.Generic;
using MatrixAPI.Data;
using MatrixAPI.Exceptions;
using MatrixAPI.Interfaces;
using MatrixMaster.Nodes;

namespace MatrixMaster.Data
{
    public class MatrixPortal : IMatrixPortal
    {
        /// <summary>
        /// Retreive a proxied node RMI identified by ID.
        /// </summary>
        /// <typeparam name="T">RMI interface</typeparam>
        /// <param name="identifier">NodeInfo identifier object</param>
        /// <returns></returns>
        public T GetNodeProxy<T>(NodeInfo identifier)
        {
            var node = NodePool.Instance.NodeForId(identifier.Id);
            if (node == null) throw new NodeNotExistException();
            return NodeProxyBuilder.GetProxyForRMI<T>(identifier);
        }

        /// <summary>
        /// Retrieve a proxied node RMI with lowest load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNodeProxy<T>()
        {
            var identifier = NodePool.Instance.NodeForRMI<T>();
            return NodeProxyBuilder.GetProxyForRMI<T>(identifier);
        }

        /// <summary>
        /// Get node list of a type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rmi"></param>
        /// <returns></returns>
        public List<NodeInfo> GetNodeList<T>(T rmi)
        {
            throw new NotImplementedException();
        }
    }
}
