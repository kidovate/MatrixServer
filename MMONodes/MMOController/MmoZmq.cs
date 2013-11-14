using System;
using ZeroMQ;

namespace MMOController
{
	public static class MmoZmq
	{
		public static ZmqContext context;

		static MmoZmq(){
			context = ZmqContext.Create();
		}
	}
}

