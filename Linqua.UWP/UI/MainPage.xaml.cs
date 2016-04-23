using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;
using Framework.PlatformServices;
using Linqua.Events;
using Linqua.Framework;
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
        private NavigationHelper navigationHelper;

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(500, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += NavigationHelper_LoadState;
            navigationHelper.SaveState += NavigationHelper_SaveState;

            if (!DesignMode.DesignModeEnabled)
            {
                DataContext = ViewModel = CompositionManager.Current.GetInstance<ICompositionFactory>().Create<MainViewModel>();
                ViewModel.View = this;

                var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();

                eventAggregator.GetEvent<EntryCreatedEvent>().Subscribe(OnEntryCreated);
                eventAggregator.GetEvent<EntryEditingFinishedEvent>().Subscribe(OnEntryEditingFinished);

                Loaded += OnLoaded;
            }
        }

        private MainViewModel ViewModel { get; }

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
            if (Log.IsDebugEnabled)
                Log.Debug("Internet connection changed. IsConnected={0}.", e.IsConnected);

            Telemetry.Client.TrackTrace("Internet Connection", TelemetrySeverityLevel.Information, new Dictionary<string, string>
            {
                {"IsAvailable", e.IsConnected.ToString()}
            });

            if (e.IsConnected)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ViewModel.RefreshAsync().FireAndForget(); }).FireAndForget();
            }
        }

        private async Task SetUpBackgroundTasksAsync()
        {
            syncBackgroundTask = await BackgroundTaskHelper.RegisterSyncTask();
            await BackgroundTaskHelper.RegisterLogsUploadTask();
            await BackgroundTaskHelper.RegisterLiveTileUpdateTask();
        }

        private void OnSyncCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            if (Log.IsTraceEnabled)
            {
                Log.Trace("Synchronization background task completed.");
            }

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ViewModel.RefreshAsync().FireAndForget(); }).FireAndForget();
        }

        public void Navigate(Type destination)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Frame.Navigate(destination); }).FireAndForget();
        }

        public void Navigate(Type destination, object parameter)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Frame.Navigate(destination, parameter); }).FireAndForget();
        }

        public void NavigateToNewWordPage()
        {
            Navigate(typeof(NewEntryPage));
        }

        public void NavigateToEntryDetails(string entryId)
        {
            Guard.NotNullOrEmpty(entryId, nameof(entryId));

            Navigate(typeof(EntryDetailsPage), entryId);
        }

        public async void FocusEntryCreationView()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { EntryEditorView.FocusInputTarget(); });
        }

        private void OnEntryCreated(EntryCreatedEvent e)
        {
        }

        private void OnEntryEditingFinished(EntryEditingFinishedEvent e)
        {
        }

        private void EntryEditorView_OnInputTargetLostFocus(object sender, EventArgs e)
        {
        }

        private void Pivot_OnPivotItemLoaded(Pivot sender, PivotItemEventArgs args)
        {
            var content = args.Item.Content as IPivotContentView;

            if (content != null)
            {
                content.OnPivotItemLoaded(this);
            }
        }

        private void Pivot_OnPivotItemUnloaded(Pivot sender, PivotItemEventArgs args)
        {
            var content = args.Item.Content as IPivotContentView;

            if (content != null)
            {
                content.OnPivotItemUnloaded(this);
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
    }
}