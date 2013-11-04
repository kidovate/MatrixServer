using System;
using System.Diagnostics;
using Castle.DynamicProxy;
using MatrixAPI.Data;
using MatrixAPI.Interfaces;
using MatrixMaster.Servers;
using log4net;

namespace MatrixMaster.Nodes
{

    public static class NodeProxyBuilder
    {
        private static ProxyGenerator generator = new ProxyGenerator();

        public static T GetProxyForRMI<T>(NodeInfo identifier, int sourceNodeID)
        {
            var proxy = generator.CreateInterfaceProxyWithoutTarget(typeof(T), new NodeProxyInterceptor(NodePool.Instance, identifier, sourceNodeID));
            return (T)proxy;
        }
    }

    public class NodeProxyInterceptor : IInterceptor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeProxyInterceptor));
        private NodePool pool;
        private NodeInfo identifier;
        private int sourceNodeId;
        
        public NodeProxyInterceptor(NodePool pool, NodeInfo identifier, int sourceNodeID)
        {
            this.pool = pool;
            this.sourceNodeId = sourceNodeID;
            this.identifier = identifier;
        }

        public void Intercept(IInvocation invocation)
        {

            //log.Debug("Node RMI invocation request " + invocation.Method.Name);
            var rmi = new NodeRMI()
                          {
                              MethodName = invocation.Method.Name,
                              NodeID = identifier.Id,
                              RequestID = new Random().Next(),
                               ReturnValue = null,
                                SNodeID = sourceNodeId
                          };
            rmi.SerializeArguments(invocation.Arguments);
            invocation.ReturnValue = pool.BlockingRMIRequest(rmi);
        }
    }
}
