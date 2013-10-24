using System;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Proxy;
using System.IO;
using System.Reflection;
namespace MatrixMaster
{
  public class NodeManager
  {
    WindsorContainer container = new WindsorContainer();
    string libFolderPath;
    string libFolderName;

    public NodeManager (string libFolderName)
    {
      libFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/"+libFolderName;
      Console.WriteLine("Initializing NodeManager: "+libFolderPath);
      this.libFolderName = libFolderName;
      if(!Directory.Exists(libFolderPath))
        Directory.CreateDirectory(libFolderPath);
    }

    /// <summary>
    /// Load libraries from the file system.
    /// </summary>
    public void Initialize(){
      container.Install(
          FromAssembly
          .InDirectory(new AssemblyFilter(libFolderName))
          //.BasedOn<INode>()
          );
    }
  }
}

