using System;
using MatrixAPI.Interfaces;
using MatrixMaster.Encryption;
using MatrixMaster.Nodes;
using MatrixMaster.Properties;
using MatrixMaster.Servers;
using log4net;

namespace MatrixMaster
{
    class MainClass
    {
        private static NodeManager manager;
        private static NodeLibraryManager nodeLibraryManager;
        private static HostInterface hostInterface;
        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));
        private static EncryptionKeyDB keyDb;
        private static NodeHost host;
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

            nodeLibraryManager = new NodeLibraryManager("CompiledNodes");
            nodeLibraryManager.Initialize();
            
            host = new NodeHost("CompiledNodes");
            log.Debug("Test download uri: "+host.GetDownloadUrl("MMOController.dll"));

            var webServer = NodeWebServer.Create(Settings.Default.HTTPPort);
            

            Console.WriteLine("Press any key to quit...");
            Console.ReadLine();

            Console.WriteLine("Shutting down...");
            webServer.Stop();
            manager.Shutdown();
            Console.WriteLine("Manager shutdown...");
            hostInterface.Shutdown();
            
        }
    }
}
