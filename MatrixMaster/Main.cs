using System;
using MatrixAPI.Interfaces;
using MatrixMaster.Encryption;
using MatrixMaster.Properties;
using MatrixMaster.Servers;
using log4net;

namespace MatrixMaster
{
    class MainClass
    {
        static NodeManager manager;
        private static HostInterface hostInterface;
        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));
        private static EncryptionKeyDB keyDb;
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("=== Matrix Master Server Launching ===");

            keyDb = new EncryptionKeyDB("EncryptionKeys");

            manager = new NodeManager("CompiledNodes");
            manager.Initialize();
            manager.LogLoadedModules();

            INodeController controller = manager.InstantiateNodeController();
            hostInterface = new HostInterface(Settings.Default.Port, controller);
            hostInterface.Startup();

            Console.WriteLine("Press any key to quit...");
            Console.ReadLine();

            Console.WriteLine("Shutting down...");
            manager.Shutdown();
            Console.WriteLine("Manager shutdown...");
            hostInterface.Shutdown();
            
        }
    }
}
