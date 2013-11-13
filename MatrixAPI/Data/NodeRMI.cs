using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MatrixAPI.Util;
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
        /// Request identifier for routing.
        /// </summary>
        [ProtoMember(6)]
        public int RequestID { get; set; }

        /// <summary>
        /// Source node for invocation;
        /// </summary>
        [ProtoMember(1)]
        public int SNodeID { get; set; }

        /// <summary>
        /// Target node for invocation
        /// </summary>
        [ProtoMember(2)]
        public int NodeID { get; set; }

        /// <summary>
        /// Target method name for invocation.
        /// </summary>
        [ProtoMember(3)]
        public string MethodName { get; set; }

        /// <summary>
        /// Each argument encoded as a byte array by BinaryFormatter
        /// </summary>
        [ProtoMember(4)]
        public byte[][] Arguments { get; set; }

        /// <summary>
        /// The return value, encoded by BinaryFormatter
        /// </summary>
        [ProtoMember(5)]
        public byte[] ReturnValue { get; set; }

        /// <summary>
        /// Serialize some arguments to <see cref="Arguments"/>.
        /// </summary>
        /// <param name="objects"></param>
        public void SerializeArguments(object[] objects)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            var data = new byte[objects.Length][];
            int i = 0;
            foreach(var obj in objects)
            {
				using (MemoryStream stream = new MemoryStream())
                {
					formatter.Serialize(stream, obj);
					data[i] = stream.ToArray();
                    BitShift.ShiftLeft(data[i]);
                }
                i++;
            }
            Arguments = data;
        }

        /// <summary>
        /// Serialize the return value to <see cref="Arguments"/>
        /// </summary>
        /// <param name="returnValue"></param>
        public void SerializeReturnValue(object returnValue)
        {
            BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream())
            {
				formatter.Serialize(stream, returnValue);
				ReturnValue = stream.ToArray();
                BitShift.ShiftLeft(ReturnValue);
            }
        }

        /// <summary>
        /// Return the arguments deserialized
        /// </summary>
        /// <returns></returns>
        public object[] DeserializeArguments()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            var data = new object[Arguments.Length];
            int i = 0;
            foreach (var arr in Arguments)
            {
				using (MemoryStream stream = new MemoryStream())
                {
                    var shifted = (byte[]) arr.Clone();
                    BitShift.ShiftRight(shifted);
					stream.Write(shifted, 0, shifted.Length);
					stream.Position = 0;
					data[i]=formatter.Deserialize(stream);
                }
                i++;
            }
            return data;
        }

        /// <summary>
        /// Deserialize the return value
        /// </summary>
        /// <returns></returns>
        public object DeserializeReturnValue()
        {
            BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream())
            {
                var shifted = (byte[])ReturnValue.Clone();
                BitShift.ShiftRight(shifted);
				stream.Write(shifted, 0, shifted.Length);
				stream.Position = 0;
				return formatter.Deserialize(stream);
            }
        }
    }
}
