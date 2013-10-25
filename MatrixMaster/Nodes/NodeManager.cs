using System;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Proxy;
using System.IO;
using System.Reflection;
using MatrixAPI.Interfaces;
using log4net;

namespace MatrixMaster
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
    }
}

