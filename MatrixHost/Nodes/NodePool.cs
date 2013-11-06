using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MatrixAPI.Data;
using MatrixAPI.Enum;
using MatrixAPI.Exceptions;
using MatrixAPI.Interfaces;
using MatrixAPI.Util;
using MatrixHost.MasterInterface;
using MatrixHost.Portal;
using ProtoBuf;

namespace MatrixHost.Nodes
{
    public class NodePool
    {
        public static NodePool Instance;

        public Dictionary<NodeInfo, INode> Nodes { get; private set; }
        public Dictionary<int, NodeRMI> rmiResponses; 
        public NodePool()
        {
            Instance = this;
            Nodes = new Dictionary<NodeInfo, INode>();
            rmiResponses = new Dictionary<int, NodeRMI>();
        }

        /// <summary>
        /// Launch a new node based on info.
        /// </summary>
        /// <param name="info"></param>
        public void LaunchNode(NodeInfo info)
        {
            var instance = NodeManager.Instance.CreateInstance(info);
            instance.Initialize(new MatrixPortal(HostClient.Instance, info.Id));
            Nodes.Add(info, instance);
        }

        public void ProcessRMI(NodeRMI rmi)
        {
            var nodeInstance = Nodes.Keys.FirstOrDefault(e => e.Id == rmi.NodeID);
            if (nodeInstance == null) return;
            var instance = Nodes[nodeInstance];
            //Find the method
            var method = nodeInstance.RMIResolvedType.GetMethod(rmi.MethodName);
            var result = method.Invoke(instance, rmi.DeserializeArguments());
            if (method.ReturnType == typeof(void)) return;

            rmi.SerializeReturnValue(result);
            HostClient.Instance.ProcessRMIResponse(rmi);
        }

        public void ShutdownNode(NodeInfo nodeInfo)
        {
            var nodeInstance = Nodes.Keys.FirstOrDefault(e => e.Id == nodeInfo.Id);
            if (nodeInstance == null) return;
            var instance = Nodes[nodeInstance];
            instance.Shutdown();
            Nodes.Remove(nodeInstance);
        }

        public object BlockingRMIRequest(NodeRMI rmi)
        {
            //Find the target node
            var targetNode = HostClient.Instance.NodeForId(rmi.NodeID);
            if (targetNode == null) throw new NodeNotExistException();
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, rmi);
                HostClient.Instance.Send(HostClient.Instance.BuildMessage(MessageIdentifier.RMIInvoke, ms.ToArray(), true));
            }
            var returnType = targetNode.RMIResolvedType.GetMethod(rmi.MethodName).ReturnType;
            if (returnType == typeof(void)) return null;
            //Wait for a response
            int time = 0;
            while (!rmiResponses.ContainsKey(rmi.RequestID))
            {
                Thread.Sleep(50);
                time++;
                if (time > 400)
                    throw new NodeRMITimeoutException();
            }
            var response = rmiResponses[rmi.RequestID];
            rmiResponses.Remove(rmi.RequestID);
            return response.DeserializeReturnValue();
        }
    }
}
