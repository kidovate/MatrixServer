using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using MatrixAPI.Data;
using MatrixAPI.Enum;
using MatrixAPI.Exceptions;
using MatrixAPI.Interfaces;
using MatrixAPI.Util;
using MatrixMaster.Data;
using MatrixMaster.Servers;
using ProtoBuf;
using log4net;

namespace MatrixMaster.Nodes
{
    /// <summary>
    /// The system's node pool contains all of the Nodes and their identifying information.
    /// </summary>
    public class NodePool
    {
        public ObservableCollection<NodeInfo> Nodes { get; private set; }
        public static NodePool Instance;
        Dictionary<int, NodeRMI> rmiResponses = new Dictionary<int, NodeRMI>();
        private static readonly ILog log = LogManager.GetLogger(typeof(NodePool));
        private static Random rnd = new Random();

        public NodePool(HostInterface hostInter)
        {
            Instance = this;
            Nodes = new ObservableCollection<NodeInfo>();
        }

        /// <summary>
        /// Register a new node in the system.
        /// </summary>
        /// <param name="info"></param>
        public void RegisterNode(NodeInfo info)
        {
            if (NodeForId(info.Id) != null) throw new IndexOutOfRangeException("node already exists");
            Nodes = new ObservableCollection<NodeInfo> {{info}};
        }

        /// <summary>
        /// Internal use - destroys a node from the index assuming it has already been cleaned up.
        /// </summary>
        /// <param name="info"></param>
        public void DestroyNode(NodeInfo info)
        {
            Nodes.Remove(NodeForId(info.Id));
            log.Debug("Removed node: "+info.Id);
            log.Debug("New node total: "+Nodes.Count);
        }

        /// <summary>
        /// Retrieve a node's information based on its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NodeInfo NodeForId(int id)
        {
            return Nodes.FirstOrDefault(e=>e.Id == id);
        }

        /// <summary>
        /// Return a node running an RMI type.
        /// </summary>
        /// <param name="rmiType"></param>
        /// <returns></returns>
        public NodeInfo NodeForRMI<T>()
        {
            return Nodes.FirstOrDefault(e => e.RMITypeName == typeof(T).FullName);
        }

        /// <summary>
        /// Check if the specified NodeInfo is in the pool.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool CheckNodeExists(NodeInfo info)
        {
            var node = Nodes.FirstOrDefault(e => e.Equals(info));
            return node != null;
        }

        /// <summary>
        /// Launch a node on any server that doesn't already have the node on it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public NodeInfo LaunchNode<T>()
        {
            var hosts = HostCache.ConnectedHosts.Where(e => e.Value.Nodes.SingleOrDefault(x=>x.RMITypeName == typeof(T).FullName) == null).ToArray();
            if (hosts.Length == 0) return null;
            return LaunchNode<T>(hosts[0].Value.Info);
        }

        /// <summary>
        /// Launch a node on a specific server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public NodeInfo LaunchNode<T>(HostInfo host)
        {
            var theHost = HostCache.FindHost(host.Id);
            var newInfo = new NodeInfo() { HostID = theHost.Id, Id = rnd.Next(), RMITypeName = typeof(T).FullName, RMIResolvedType = typeof(T) };
            theHost.Nodes.Add(newInfo);
            Nodes.Add(newInfo);
            log.Debug("Node launched, ID: "+newInfo.Id+" RMI: "+newInfo.RMITypeName+" Host Node Count: "+theHost.Nodes.Count);
            log.Debug("New node total: "+Nodes.Count);
            return newInfo;
        }

        /// <summary>
        /// Gets an ID given a INode instance.
        /// </summary>
        /// <param name="callingNode"></param>
        /// <returns></returns>
        public int IdForNode(INode callingNode)
        {
            //On the server this is just the default identity for the controller
            return 0;
        }

        /// <summary>
        /// Called by the HostInterface when an RMI response is received
        /// </summary>
        /// <param name="rmi"></param>
        public void HandleRMIResponse(NodeRMI rmi)
        {
            rmiResponses.Add(rmi.RequestID, rmi);
        }

        /// <summary>
        /// Performs a blocking RMI routing and execution request.
        /// </summary>
        /// <param name="rmi"></param>
        /// <returns></returns>
        public object BlockingRMIRequest(NodeRMI rmi)
        {
            //Find the target node
            var targetNode = NodeForId(rmi.NodeID);
            if(targetNode == null) throw new NodeNotExistException();
            //Find the target host
            var host = HostCache.FindHost(targetNode.HostID);
            using(var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, rmi);
                HostInterface.Instance.SendTo(host.Info, host.BuildMessage(MessageIdentifier.RMIInvoke, ms.ToArray()));
            }
            if (targetNode.RMIResolvedType.GetMethod(rmi.MethodName).ReturnType == typeof(void)) return null;
            //Wait for a response
            int time = 0;
            while(!rmiResponses.ContainsKey(rmi.RequestID))
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

        public NodeInfo[] GetNodeList()
        {
            return Nodes.ToArray();
        }

        public void ShutdownNode(NodeInfo node)
        {
            var actualNode = NodeForId(node.Id);
            if (actualNode == null) return;
            var host = HostCache.FindHost(node.HostID);
            if (host == null) return;
            host.Nodes.Remove(actualNode);
        }
    }
}
