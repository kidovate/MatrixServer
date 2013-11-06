using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Data;
using MatrixAPI.Exceptions;
using MatrixAPI.Interfaces;
using MatrixHost.MasterInterface;
using MatrixHost.Nodes;

namespace MatrixHost.Portal
{
    /// <summary>
    /// Implementation of the matrix portal
    /// </summary>
    public class MatrixPortal : IMatrixPortal
    {
        private int nodeid;
        private HostClient clientInterface;
        public MatrixPortal(HostClient clientInterface, int nodeid)
        {
            this.nodeid = nodeid;
            this.clientInterface = clientInterface;
        }
        /// <summary>
        /// Retreive a proxied node RMI identified by ID.
        /// </summary>
        /// <typeparam name="T">RMI interface</typeparam>
        /// <param name="identifier">NodeInfo identifier object</param>
        /// <returns></returns>
        public T GetNodeProxy<T>(NodeInfo identifier)
        {
            var type = typeof (T);
            if (!type.IsInterface || !type.IsAssignableFrom(typeof(IRMIInterface)) || type.FullName != identifier.RMITypeName) throw new InvalidNodeTypeException();

            bool isValid = clientInterface.AsyncNodeVerify(typeof(T), identifier);
            if(!isValid) throw new InvalidNodeTypeException();

            return NodeProxyBuilder.GetProxyForRMI<T>(identifier, nodeid);
        }

        /// <summary>
        /// Retrieve a proxied node RMI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNodeProxy<T>()
        {
            var identifier = clientInterface.NodeForType<T>();
            if(identifier == null) throw new NodeNotExistException();
            return NodeProxyBuilder.GetProxyForRMI<T>(identifier, nodeid);
        }

        /// <summary>
        /// Get node list of a type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rmi"></param>
        /// <returns></returns>
        public NodeInfo[] GetNodeList<T>(T rmi)
        {
            return clientInterface.AllNodeForType<T>();
        }
    }
}
