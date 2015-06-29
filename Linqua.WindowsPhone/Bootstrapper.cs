using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using Framework;
using Framework.Logging;
using Framework.MefExtensions;
using MetroLog;
using MetroLog.Targets;
using FileStreamingTarget = Linqua.Logging.FileStreamingTarget;

namespace Linqua
{
    public class Bootstrapper
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<Bootstrapper>();

        public void Run(App application)
        {
			ConfigureLogger();

			if (Log.IsInfoEnabled)
				Log.Info("Application Launched. ============================================================================================");

			if (Log.IsInfoEnabled)
				Log.Info("Bootstrapper sequence started.");
			
            ConfigureMef();
	        InitializeGlobalServices();

	        if (Log.IsInfoEnabled)
				Log.Info("Bootstrapper sequence completed.");
        }

	    private void ConfigureLogger()
	    {
			var configuration = new LoggingConfiguration();
#if DEBUG
			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget(new LoggingLayout()));
#endif
		    configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, FileStreamingTarget.Instance);

			configuration.IsEnabled = true;
			
			LogManagerFactory.DefaultConfiguration = configuration;

			// setup the global crash handler...
			GlobalCrashHandler.Configure();
	    }

	    private static void ConfigureMef()
	    {
		    var mvvmConventions = new ConventionBuilder();

		    mvvmConventions.ForTypesDerivedFrom<ViewModelBase>()
		                   .Export();

		    ViewLocator.BuildMefConventions(mvvmConventions);

		    var configuration = new ContainerConfiguration()
			    .WithAssembly(typeof(App).GetTypeInfo().Assembly, mvvmConventions)
			    .WithAssembly(typeof(AppPortable).GetTypeInfo().Assembly, mvvmConventions)
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