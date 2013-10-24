using System;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Proxy;
using System.IO;
using System.Reflection;
using MatrixAPI.Interfaces;
namespace MatrixMaster
{
  public class NodeManager
  {
    WindsorContainer container = new WindsorContainer();
    string libFolderPath;
    public static string libFolderName;

    public NodeManager (string folderName)
    {
	  libFolderName = folderName;
      libFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/"+libFolderName;
      Console.WriteLine("Initializing NodeManager: "+libFolderPath);
      if(!Directory.Exists(libFolderPath))
        Directory.CreateDirectory(libFolderPath);
    }

    /// <summary>
    /// Load libraries from the file system.
    /// </summary>
    public void Initialize(){
      container.Install(
      		new NodeInstaller()
          );
    }

	public void LogLoadedModules ()
	{
	    Console.WriteLine("Loaded node controllers:");
		var handlers = container.Kernel.GetHandlers(typeof(INodeController));
	    foreach(var handler in handlers){
			Console.WriteLine ("  --> "+handler.ComponentModel.Implementation.FullName);
		}
			
		Console.WriteLine("Loaded node implementations:");
		handlers = container.Kernel.GetHandlers(typeof(INode));
	    foreach(var handler in handlers){
			Console.WriteLine ("  --> "+handler.ComponentModel.Implementation.FullName);
		}
	}
  }
}

