using System;
using log4net;

namespace MatrixMaster
{
    class MainClass
    {
        static NodeManager manager;
        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));
        
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("=== Matrix Master Server Launching ===");

            manager = new NodeManager("CompiledNodes");
            manager.Initialize();
            manager.LogLoadedModules();



            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}
