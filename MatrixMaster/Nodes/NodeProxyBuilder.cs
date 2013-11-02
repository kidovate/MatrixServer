using Castle.DynamicProxy;
using MatrixAPI.Data;
using log4net;

namespace MatrixMaster.Nodes
{

    public static class NodeProxyBuilder
    {
        private static ProxyGenerator generator = new ProxyGenerator();

        public static T GetProxyForRMI<T>(NodeInfo identifier)
        {
            var proxy = generator.CreateInterfaceProxyWithoutTarget(typeof(T), new NodeProxyInterceptor(NodePool.Instance, identifier));
            return (T)proxy;
        }
    }

    public class NodeProxyInterceptor : IInterceptor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeProxyInterceptor));
        private NodePool pool;
        private NodeInfo identifier;
        
        public NodeProxyInterceptor(NodePool pool, NodeInfo identifier)
        {
            this.pool = pool;
            this.identifier = identifier;
        }

        public void Intercept(IInvocation invocation)
        {
            log.Debug("Node RMI invocation request " + invocation.Method.Name);
            invocation.ReturnValue = true;
        }
    }
}
