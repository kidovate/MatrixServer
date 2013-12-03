using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MatrixAPI.Encryption;
using MatrixHost.MasterInterface;
using MatrixHost.Nodes;
using MatrixHost.Properties;
using log4net;

namespace MatrixHost
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static HostClient client;
        private static NodeLibraryManager nodeLibraryManager;
        private static NodeManager manager;
        private static NodePool pool;
        private static bool restart;
        private static bool running;

        public static void Main(string[] args)
        {
            running = true;
            log4net.Config.XmlConfigurator.Configure();
            log.Info("=== Matrix Host Server Launching ===");

            log.Debug("Searching for encryption key "+Settings.Default.KeyFile+"...");
            if(!File.Exists(Settings.Default.KeyFile))
            {
                log.Error("No encryption key! Exiting...");
                Console.ReadLine();
                return;
            }

            byte[] hash;
            AES encrypt;
            //Hash the file to a all lowercase string
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Settings.Default.KeyFile))
                {
                    hash = md5.ComputeHash(stream);
                    encrypt = new AES(Encoding.UTF8, Settings.Default.KeyFile);
                }
            }


            client = new HostClient(Settings.Default.MasterIP, Settings.Default.MasterPort, encrypt, hash);
            nodeLibraryManager = new NodeLibraryManager("CompiledNodes", client);
            pool = new NodePool();
            client.Startup();

            while(running)
            {
                Thread.Sleep(50);
            }

            client.Shutdown();
            if(restart)
            {
                restart = false;
                Main(args);
            }
        }

        public static void ForceRestart()
        {
            running = false;
            restart = true;
        }
    }
}
