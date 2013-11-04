using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MatrixAPI.Data;
using MatrixAPI.Interfaces;
using MatrixHost.MasterInterface;

namespace MatrixHost.Nodes
{
    public class NodePool
    {
        public static NodePool Instance;

        private Dictionary<NodeInfo, INode> nodes; 

        public NodePool()
        {
            Instance = this;
            nodes = new Dictionary<NodeInfo, INode>();
        }

        /// <summary>
        /// Launch a new node based on info.
        /// </summary>
        /// <param name="info"></param>
        public void LaunchNode(NodeInfo info)
        {
            var instance = NodeManager.Instance.CreateInstance(info);
            nodes.Add(info, instance);
        }

        public void AsyncProcessRMI(NodeRMI rmi)
        {
            //Spawn a new thread
            ThreadPool.QueueUserWorkItem(ProcessRMI, rmi);
        }

        public void ProcessRMI(object ormi)
        {
            var rmi = (NodeRMI) ormi;
            var nodeInstance = nodes.Keys.FirstOrDefault(e => e.Id == rmi.NodeID);
            if (nodeInstance == null) return;
            var instance = nodes[nodeInstance];
            //Find the method
            var method = nodeInstance.RMIResolvedType.GetMethod(rmi.MethodName);
            var result = method.Invoke(instance, rmi.DeserializeArguments());
            if (method.ReturnType == typeof(void)) return;

            rmi.SerializeReturnValue(result);
            HostClient.Instance.ProcessRMIResponse(rmi);
        }
    }
}
