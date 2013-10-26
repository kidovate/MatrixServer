using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;
using SocketType = System.Net.Sockets.SocketType;

namespace SocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket server = context.CreateSocket(ZeroMQ.SocketType.ROUTER), client = context.CreateSocket(ZeroMQ.SocketType.DEALER))
            {
                server.Bind("inproc://example");

                client.Identity = Encoding.UTF8.GetBytes("TestIdentity");
                client.Connect("inproc://example");

                client.Send("Data", Encoding.UTF8);

                var message = server.ReceiveMessage();
                var identity = Encoding.UTF8.GetString(message[0].Buffer);
                var data = Encoding.UTF8.GetString(message[1].Buffer);
                Console.WriteLine("Identity: "+identity+"\nData: "+data);

                Console.ReadLine();
            }
        }
    }
}
