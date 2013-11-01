using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Data;
using MatrixAPI.Exceptions;
using MatrixAPI.Interfaces;
using MatrixHost.MasterInterface;

namespace MatrixHost.Portal
{
    /// <summary>
    /// Implementation of the matrix portal
    /// </summary>
    public class MatrixPortal : IMatrixPortal
    {

        private HostClient clientInterface;
        public MatrixPortal(HostClient clientInterface)
        {
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
            if (!type.IsInterface || !type.IsAssignableFrom(typeof(IRMIInterface)) || type.Name != identifier.RMITypeName) throw new InvalidNodeTypeException();

            bool isValid = clientInterface.AsyncNodeVerify(typeof(T), identifier);
            if(!isValid) throw new InvalidNodeTypeException();
            throw new NotImplementedException();

        }

        /// <summary>
        /// Retrieve a proxied node RMI with lowest load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNodeProxy<T>()
        {
            throw new NotImplementedException();
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
