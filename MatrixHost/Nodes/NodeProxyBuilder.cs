﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using MatrixAPI.Data;
using MatrixHost.MasterInterface;
using log4net;

namespace MatrixHost.Nodes
{

    public static class NodeProxyBuilder
    {
        private static ProxyGenerator generator = new ProxyGenerator();
        public static T GetProxyForRMI<T>(NodeInfo identifier)
        {
            var proxy = generator.CreateInterfaceProxyWithoutTarget(typeof(T), new NodeProxyInterceptor(HostClient.Instance, identifier));
            return (T) proxy;
        }
    }

    public class NodeProxyInterceptor : IInterceptor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeProxyInterceptor));
        private HostClient client;
        private NodeInfo identifier;
        
        public NodeProxyInterceptor(HostClient clientInterface, NodeInfo identifier)
        {
            client = clientInterface;
            this.identifier = identifier;
        }

        public void Intercept(IInvocation invocation)
        {
            log.Debug("Node RMI invocation request " + invocation.Method.Name);
        }
    }
}
