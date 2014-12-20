using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using Framework;
using Framework.MefExtensions;

namespace Linqua
{
    public class Bootstrapper
    {
        public void Run(App application)
        {
            var mvvmConventions = new ConventionBuilder();

            mvvmConventions.ForTypesDerivedFrom<ViewModelBase>()
                           .Export();
	       

            ViewLocator.BuildMefConventions(mvvmConventions);

            var configuration = new ContainerConfiguration()
                .WithAssembly(typeof (App).GetTypeInfo().Assembly, mvvmConventions)
                .WithAssembly(typeof (AppPortable).GetTypeInfo().Assembly, mvvmConventions)
                .WithAssembly(typeof (FrameworkPortable).GetTypeInfo().Assembly)
                .WithAssembly(typeof (FrameworkPhone).GetTypeInfo().Assembly)
                .WithProvider(new DefaultExportDescriptorProvider());
            
            var container = configuration.CreateContainer();

			CompositionManager.Initialize(container);

            App.CompositionManager = CompositionManager.Current;
        }
    }
}