using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;
using Linqua.Events;
using MetroLog;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Linqua.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
	public sealed partial class MainPage : Page, IMainView, IPivotHostView
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainPage>();

	    private BackgroundTaskRegistration syncBackgroundTask;

	    public MainPage()
	    {
            this.InitializeComponent();
			
            this.NavigationCacheMode = NavigationCacheMode.Required;
	        
			if (!DesignMode.DesignModeEnabled)
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
			ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

		    if (DesignMode.DesignModeEnabled) return;
			
		    await SetUpBackgroundTasksAsync();

		    SubscribeToEvents();

		    ViewModel.Initialize();
			 
		    // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

	    protected override void OnNavigatedFrom(NavigationEventArgs e)
	    {
		    UnsubscribeFromEvents();
	    }

	    private void UnsubscribeFromEvents()
	    {
			ConnectionHelper.InternetConnectionChanged -= OnInternetConnectionChanged;

		    if (syncBackgroundTask != null)
		    {
			    syncBackgroundTask.Completed -= OnSyncCompleted;
		    }
	    }

	    private void SubscribeToEvents()
	    {
		    ConnectionHelper.InternetConnectionChanged += OnInternetConnectionChanged;

			if (syncBackgroundTask != null)
			{
				syncBackgroundTask.Completed += OnSyncCompleted;
			}
	    }

	    private void OnInternetConnectionChanged(object sender, InternetConnectionChangedEventArgs e)
	    {
		    if (Log.IsInfoEnabled)
				Log.Info("Internet connection changed. IsConnected={0}.", e.IsConnected);

		    if (e.IsConnected)
		    {
				Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					ViewModel.RefreshAsync().FireAndForget();
				}).FireAndForget();
		    }
	    }

	    private async Task SetUpBackgroundTasksAsync()
	    {
		    syncBackgroundTask = await BackgroundTaskHelper.RegisterSyncTask();
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
		    }).FireAndForget();
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

	    public void NavigateToEntryDetails(string entryId)
	    {
		    Guard.NotNullOrEmpty(entryId, () => entryId);

		    Navigate(typeof(EntryDetailsPage), entryId);
	    }

	    public async void FocusEntryCreationView()
	    {
		    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
		    {
				EntryCreationView.FocusInputTarget();
		    });
	    }

	    private void OnEntryCreated(EntryCreatedEvent e)
	    {
		    //Pivot.Focus(FocusState.Programmatic);

		    //var userControl = (UserControl)RandomEntryListView.Content;

		    //if (userControl != null)
		    //{
		    //	userControl.Focus(FocusState.Programmatic);
		    //}
	    }

	    private void EntryCreationView_OnInputTargetLostFocus(object sender, EventArgs e)
	    {
		    if (string.IsNullOrEmpty(ViewModel.EntryCreationViewModel.EntryText))
		    {
			    ViewModel.IsEntryCreationViewVisible = false;
		    }
	    }

		private void Pivot_OnPivotItemLoaded(Pivot sender, PivotItemEventArgs args)
		{
			var content = args.Item.Content as IPivotContentView;

			if (content != null)
			{
				content.OnPivotItemLoaded(this);
			}

			if (Equals(args.Item.Tag, "RendomEntryList"))
			{
				ToggleShowHideLearnedEntriesButton.Visibility = Visibility.Collapsed;
			}
		}

		private void Pivot_OnPivotItemUnloaded(Pivot sender, PivotItemEventArgs args)
		{
			var content = args.Item.Content as IPivotContentView;

			if (content != null)
			{
				content.OnPivotItemUnloaded(this);
			}

			if (Equals(args.Item.Tag, "RendomEntryList"))
			{
				ToggleShowHideLearnedEntriesButton.Visibility = Visibility.Visible;
			}
		}
    }
}
