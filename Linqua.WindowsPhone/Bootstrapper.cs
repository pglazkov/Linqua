using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Framework;
using Framework.MefExtensions;
using Linqua.Logging;
using MetroLog;

namespace Linqua
{
    public class Bootstrapper
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<Bootstrapper>();

        public void Run(App application)
        {

			ConfigureLogger();

			Log.Debug("Application Launched. ============================================================================================");
			Log.Debug("Bootstrapper sequence started.");

			ConfigureMef();
	        InitializeGlobalServices();

			Log.Debug("Bootstrapper sequence completed.");
		}

	    private void ConfigureLogger()
	    {
            LoggerHelper.SetupLogger();
        }

	    private static void ConfigureMef()
	    {
		    var conventions = new ConventionBuilder();

		    conventions.ForTypesDerivedFrom<ViewModelBase>()
		               .Export();

		    conventions.ForTypesMatching(x => x.Name.EndsWith("Service"))
		               .ExportInterfaces();

		    ViewLocator.BuildMefConventions(conventions);

		    var configuration = new ContainerConfiguration()
			    .WithAssembly(typeof(App).GetTypeInfo().Assembly, conventions)
			    .WithAssembly(typeof(AppPortable).GetTypeInfo().Assembly, conventions)
			    .WithAssembly(typeof(FrameworkPortable).GetTypeInfo().Assembly)
			    .WithAssembly(typeof(FrameworkPhone).GetTypeInfo().Assembly)
			    .WithProvider(new DefaultExportDescriptorProvider());

		    var container = configuration.CreateContainer();

		    CompositionManager.Initialize(container);

		    App.CompositionManager = CompositionManager.Current;
	    }

		private void InitializeGlobalServices()
		{
			var appController = CompositionManager.Current.GetInstance<ApplicationController>();
			appController.Initialize();
		}
    }
}