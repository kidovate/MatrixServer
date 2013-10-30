using Castle.Windsor;
using System.IO;
using System.Reflection;
using MatrixAPI.Interfaces;
using log4net;

namespace MatrixHost.Nodes
{
    public class NodeManager
    {
        WindsorContainer container = new WindsorContainer();
        string libFolderPath;
        public static string libFolderName;
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeManager));

        public NodeManager(string folderName)
        {
            libFolderName = folderName;
            libFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/" + libFolderName;
            log.Info("Initializing Node library loader...");
            if (!Directory.Exists(libFolderPath))
                Directory.CreateDirectory(libFolderPath);
        }

        /// <summary>
        /// Load libraries from the file system.
        /// </summary>
        public void Initialize()
        {
            log.Info("Loading node libraries from filesystem...");
            container.Install(
                  new NodeInstaller()
                );
            log.Debug("Node libraries loaded.");
        }

        public void LogLoadedModules()
        {
            log.Debug("Loaded node controllers:");
            var handlers = container.Kernel.GetHandlers(typeof(INodeController));
            foreach (var handler in handlers)
            {
                log.Debug("  --> " + handler.ComponentModel.Implementation.FullName);
            }

            log.Debug("Loaded node implementations:");
            handlers = container.Kernel.GetHandlers(typeof(INode));
            foreach (var handler in handlers)
            {
                log.Debug("  --> " + handler.ComponentModel.Implementation.FullName);
            }
        }

        /// <summary>
        /// Pick a node controller and return it.
        /// </summary>
        /// <returns></returns>
        public INodeController InstantiateNodeController()
        {
            var handlers = container.Kernel.GetHandlers(typeof (INodeController));
            if (handlers.Length == 0)
                return null;
            //Create an instance
            var instance = container.Resolve<INodeController>();
            log.Debug("Instantiated NodeController: "+instance.GetType().FullName);
            return instance;
        }

        public void Shutdown()
        {
            //Shutdown all instances
        }
    }
}

