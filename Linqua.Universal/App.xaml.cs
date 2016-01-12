using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;
using Framework.PlatformServices;
using Linqua.Framework;
using Linqua.Persistence;
using MetroLog;
using Microsoft.ApplicationInsights;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Linqua
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
		private readonly ILogger log;
	    private RootFrameNavigationHelper frameNavigationHelper;

#if WINDOWS_PHONE_APP
        //private TransitionCollection transitions;
#endif
		public static ICompositionManager CompositionManager { get; internal set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
#if !DEBUG
			WindowsAppInitializer.InitializeAsync(WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.PageView);
#endif

			InitializeComponent();
            Suspending += OnSuspending;

            //CoreApplication.UnhandledErrorDetected += // TODO
	        UnhandledException += OnUnhandledException;

			var bootstrapper = new Bootstrapper();

			bootstrapper.Run(this);

			log = LogManagerFactory.DefaultLogManager.GetLogger<App>();
        }

	    /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
		    Telemetry.Client.TrackEvent("App Launched.", new Dictionary<string, string>
		    {
			    {"DeviceId", DeviceInfo.DeviceId}
		    });

			log.Info("Internet Connection: " + ConnectionHelper.IsConnectedToInternet);

			Telemetry.Client.TrackTrace("Internet Connection", TelemetrySeverityLevel.Information, new Dictionary<string, string>
			{
				{"IsAvailable", ConnectionHelper.IsConnectedToInternet.ToString() }
			});

			Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
				frameNavigationHelper = new RootFrameNavigationHelper(rootFrame);

				// Associate the frame with a SuspensionManager key.
				SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

				// Register the main navigation frame with the application controller.
	            var applicationController = CompositionManager.GetInstance<ApplicationController>();
				applicationController.RegisterFrame(rootFrame);
				
                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
					// Restore the saved session state only when appropriate.
					try
					{
						await SuspensionManager.RestoreAsync();
					}
					catch (SuspensionManagerException)
					{
						// Something went wrong restoring state.
						// Assume there is no state and continue.
					}
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
				//if (rootFrame.ContentTransitions != null)
				//{
				//	transitions = new TransitionCollection();
				//	foreach (var c in rootFrame.ContentTransitions)
				//	{
				//		transitions.Add(c);
				//	}
				//}

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;
#endif

	            StartupAsync(rootFrame, e).FireAndForget();
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private static async Task StartupAsync(Frame rootFrame, LaunchActivatedEventArgs e)
        {
            var startupWorkflow = new StartupWorkflow(rootFrame, e.Arguments);

            var success = await startupWorkflow.RunAsync();

            if (!success)
            {
                Current.Exit();
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
			var rootFrame = (Frame)sender;
            //rootFrame.ContentTransitions = transitions ?? new TransitionCollection { new NavigationThemeTransition() };
            rootFrame.Navigated -= RootFrame_FirstNavigated;
        }
#endif

		protected override void OnActivated(IActivatedEventArgs args)
		{
			log.Debug("Application Activated.");

			// Windows Phone 8.1 requires you to handle the respose from the WebAuthenticationBroker.
#if WINDOWS_PHONE_APP
			//if (args.Kind == ActivationKind.WebAuthenticationBrokerContinuation)
			//{
			//	// Completes the sign-in process started by LoginAsync.
			//	// Change 'MobileService' to the name of your MobileServiceClient instance. 
			//	MobileService.Client.LoginComplete(args as WebAuthenticationBrokerContinuationEventArgs);
			//}
#endif

			base.OnActivated(args);
		}

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

			if (log.IsDebugEnabled)
				await ((ILoggerAsync)log).DebugAsync("Application is suspending.");

			await SuspensionManager.SaveAsync();
            await OfflineHelper.AwaitPendingSync();
			await LazyFlushManager.FlushAllAsync(new LogWriteContext());

			deferral.Complete();
        }

		private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// say we've handled this one. this allows our FATAL write to complete.
			e.Handled = true;

			var ex = ExceptionUtils.UnwrapException(e.Exception);

			Telemetry.Client.TrackCrash(ex);

			await ((ILoggerAsync)log).FatalAsync("CRASH!", ex);

			// if we're aborting, fake a suspend to flush the targets...
			await LazyFlushManager.FlushAllAsync(new LogWriteContext());

			ApplicationData.Current.LocalSettings.Values[LocalSettingsKeys.LogsUploadPending] = true;

			// abort the app here...
			Current.Exit();
		}
    }
}