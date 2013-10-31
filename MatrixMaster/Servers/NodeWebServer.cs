using System.IO;
using System.Net;
using System.Text;
using Griffin.Networking.Buffers;
using Griffin.Networking.Messaging;
using Griffin.Networking.Protocol.Http;
using Griffin.Networking.Servers;
using MatrixMaster.Nodes;
using log4net;

namespace MatrixMaster.Servers
{
    /// <summary>
    /// Hosts the nodes for download.
    /// </summary>
    public class NodeWebServer : IServiceFactory
    {
        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static MessagingServer Create(int port)
        {
            var server = new MessagingServer(new NodeWebServerFactory(), new MessagingServerConfiguration(new HttpMessageFactory()));
            server.Start(new IPEndPoint(IPAddress.Any, port));
            return server;
        }

        class NodeWebServerService : HttpService
        {
            private static readonly  BufferSliceStack Stack =new BufferSliceStack(50, 32000);
            private static readonly ILog log = LogManager.GetLogger(typeof(NodeWebServerService));
            public NodeWebServerService() : base(Stack)
            {
                
            }

            public override void Dispose()
            {
            }

            public override void OnRequest(Griffin.Networking.Protocol.Http.Protocol.IRequest request)
            {
                //log.Debug("HTTP request for " + request.Uri.LocalPath);

                var resultStream = NodeHost.Instance.GetDataForUri(request.Uri.LocalPath.Replace("/", ""));
                if (resultStream == null)
                {
                    var errorResp = request.CreateResponse(HttpStatusCode.NotFound, "URI not found.");
                    errorResp.Body = new MemoryStream();
                    errorResp.ContentType = "text/plain";
                    var errBuffer = Encoding.UTF8.GetBytes("URI not found.");
                    errorResp.Body.Write(errBuffer, 0, errBuffer.Length);
                    errorResp.Body.Position = 0;
                    Send(errorResp);
                    return;
                }

                var response = request.CreateResponse(HttpStatusCode.OK, "Lib download stream");
                response.Body = new MemoryStream();
                response.Body.Write(resultStream, 0, (int)resultStream.Length);
                response.ContentType = "application/octet-stream";
                response.Body.Position = 0;
                Send(response);

            }
        }

        class NodeWebServerFactory : IServiceFactory
        {
            public INetworkService CreateClient(EndPoint remoteEndPoint)
            {
                return new NodeWebServerService();
            }
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="remoteEndPoint">IP address of the remote end point</param>
        /// <returns>
        /// Created client
        /// </returns>
        public INetworkService CreateClient(EndPoint remoteEndPoint)
        {
            return new NodeWebServerService();
        }
    }

    
}
