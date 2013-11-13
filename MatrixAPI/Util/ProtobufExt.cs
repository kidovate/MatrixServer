using System;
using System.IO;
using ProtoBuf;

namespace MatrixAPI.Util
{
	public static class ProtobufExt
	{
		/// <summary>
		/// Deserializes using ProtoBuf and a MemoryStream.
		/// </summary>
		/// <param name="t">The input data arary.</param>
		/// <typeparam name="T">Type to deserialize.</typeparam>
		public static T Deserialize<T>(this byte[] t)
		{
            if (!typeof(T).IsArray && !Attribute.IsDefined(typeof(T), typeof(ProtoContractAttribute)))
            {
				throw new Exception("You can only deserialize ProtoBuf objects.");
			}

			using(var ms = new MemoryStream()){
				ms.Write(t, 0, t.Length);
				ms.Position = 0;
				return Serializer.Deserialize<T>(ms);
			}
		}

		/// <summary>
		/// Serializes an instance to a byte array using ProtoBuf.
		/// </summary>
		/// <param name="instance">Instance.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static byte[] Serialize<T>(this T instance){
			if(!Attribute.IsDefined(typeof(T), typeof(ProtoContractAttribute)))
			{
				throw new Exception("You can only serialize ProtoBuf objects.");
			}

			using(var ms = new MemoryStream()){
				Serializer.Serialize(ms, instance);
				return ms.ToArray();
			}
		}
	}
}

