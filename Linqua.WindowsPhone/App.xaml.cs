using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Framework;
using Linqua.Framework;
using Linqua.Persistence;
using MetroLog;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Linqua
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
		private readonly ILogger log;

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
			if (log.IsInfoEnabled)
				log.Info("OnLaunched");

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

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
			if (log.IsInfoEnabled)
				log.Info("Application Activated.");

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

			if (log.IsInfoEnabled)
				await ((ILoggerAsync)log).InfoAsync("Application is suspending.");

			await SuspensionManager.SaveAsync();
            await OfflineHelper.AwaitPendingSync();

            deferral.Complete();
        }

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			log.Fatal(e.Message, e.Exception);
		}
    }
}