using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;
using Framework.PlatformServices;
using Linqua.BackgroundTasks;
using Linqua.Events;
using MetroLog;
using Microsoft.WindowsAzure.MobileServices;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Linqua
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
	public sealed partial class MainPage : Page, IMainView
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainPage>();

	    private BackgroundTaskRegistration syncBackgroundTask;

	    public MainPage()
	    {
            this.InitializeComponent();
			
            this.NavigationCacheMode = NavigationCacheMode.Required;
	        
			if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
				DataContext = ViewModel = CompositionManager.Current.GetInstance<ICompositionFactory>().Create<MainViewModel>();
				ViewModel.View = this;

				var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();

				eventAggregator.GetEvent<EntryCreatedEvent>().Subscribe(OnEntryCreated);

				Loaded += OnLoaded;
			}
        }

	    private MainViewModel ViewModel { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
		}

	    /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
		    if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
		    {
				if (Log.IsDebugEnabled)
					Log.Debug("Registering SyncTask background task.");

			    syncBackgroundTask = await BackgroundTaskHelper.RegisterSyncTask();

				if (Log.IsDebugEnabled)
					Log.Debug("Background task registered. TaskId: {0}", syncBackgroundTask.TaskId);

			    syncBackgroundTask.Completed += OnSyncCompleted;

			    ViewModel.Initialize();
		    }

		    // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

	    private void OnSyncCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
	    {
			if (Log.IsTraceEnabled)
			{
				Log.Trace("Synchronization background task completed.");
			}

		    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
		    {
			    ViewModel.RefreshAsync().FireAndForget();
		    });
	    }

	    public bool Navigate(Type destination)
	    {
		    return Frame.Navigate(destination);
	    }

	    public bool Navigate(Type destination, object parameter)
	    {
		    return Frame.Navigate(destination, parameter);
	    }

	    public void NavigateToNewWordPage()
	    {
		    Navigate(typeof (NewEntryPage));
	    }

		private void OnEntryCreated(EntryCreatedEvent e)
		{
			var userControl = (UserControl)EntryListView.Content;

			if (userControl != null)
			{
				userControl.Focus(FocusState.Programmatic);
			}
		}
    }
}
