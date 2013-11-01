using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using MatrixHost.MasterInterface;
using log4net;

namespace MatrixHost.Nodes
{

    public class NodeProxyBuilder
    {
    }

    public class NodeProxyInterceptor : IInterceptor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeProxyInterceptor));
        private HostClient client;
        
        public NodeProxyInterceptor(HostClient clientInterface)
        {
            client = clientInterface;
        }

        public void Intercept(IInvocation invocation)
        {
            log.Debug("Node RMI invocation request "+invocation.Method.Name);
        }
    }
}
