using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Data;
using MatrixAPI.Interfaces;

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
            var instance = NodeManager.Instance.CreateInstance(info.RMITypeName);
            nodes.Add(info, instance);
        }
    }
}
