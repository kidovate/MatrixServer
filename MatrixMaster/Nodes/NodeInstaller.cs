using MatrixAPI.Interfaces;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;

namespace MatrixMaster.Nodes
{
	public class NodeInstaller : IWindsorInstaller
{
   public void Install(IWindsorContainer container, IConfigurationStore store)
   {
      container.Register(Classes
			                .FromAssemblyInDirectory(new AssemblyFilter(NodeManager.libFolderName))
                            .BasedOn<INodeController>().OrBasedOn(typeof(INode)).WithService.FromInterface()
			                
			     );
   }
}
}

