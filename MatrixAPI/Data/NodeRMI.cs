using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace MatrixAPI.Data
{
    /// <summary>
    /// A serialized node remote method invocation.
    /// </summary>
    [ProtoContract]
    public class NodeRMI
    {
        /// <summary>
        /// Target node for invocation
        /// </summary>
        [ProtoMember(1)]
        public int NodeID;

        /// <summary>
        /// Target method name for invocation.
        /// </summary>
        [ProtoMember(2)]
        public string MethodName;

    }
}
