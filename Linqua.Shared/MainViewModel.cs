using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Enumeration;
using Framework;
using Framework.PlatformServices;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
    public class MainViewModel : ViewModelBase
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainViewModel>();

	    private readonly IDataStore storage;
	    private readonly IEventAggregator eventAggregator;
	    private readonly IStatusBusyService statusBusyService;
	    private EntryListViewModel entryListViewModel;
	    private bool isLoadingEntries;

	    public MainViewModel()
	    {
			if (DesignTimeDetection.IsInDesignTool)
			{
				EntryListViewModel = new EntryListViewModel(FakeData.FakeWords);
				EntryCreationViewModel = new EntryCreationViewModel(DesignTimeHelper.EventAggregator);
			}

			SyncCommand = new DelegateCommand(ForceSync);
			SendLogsCommand = new DelegateCommand(SendLogs);
			AddWordCommand = new DelegateCommand(AddWord);
	    }

	    public MainViewModel(ICompositionFactory compositionFactory, IDataStore storage, IEventAggregator eventAggregator, IStatusBusyService statusBusyService)
			: this()
	    {
		    Guard.NotNull(compositionFactory, () => compositionFactory);
		    Guard.NotNull(storage, () => storage);
		    Guard.NotNull(eventAggregator, () => eventAggregator);
		    Guard.NotNull(statusBusyService, () => statusBusyService);

		    this.storage = storage;
		    this.eventAggregator = eventAggregator;
		    this.statusBusyService = statusBusyService;

		    CompositionFactory = compositionFactory;

			EntryListViewModel = compositionFactory.Create<EntryListViewModel>();
		    EntryCreationViewModel = compositionFactory.Create<EntryCreationViewModel>();

		    eventAggregator.GetEvent<EntryCreationRequestedEvent>().Subscribe(OnEntryCreationRequested);
		    eventAggregator.GetEvent<EntryDeletionRequestedEvent>().Subscribe(OnEntryDeletionRequested);
		    eventAggregator.GetEvent<EntryIsLearntChangedEvent>().Subscribe(OnEntryIsLearntChanged);
	    }

	    public ICommand SendLogsCommand { get; private set; }
	    public ICommand AddWordCommand { get; private set; }
		public ICommand SyncCommand { get; private set; }

		public IMainView View { get; set; }

		public EntryCreationViewModel EntryCreationViewModel { get; private set; }

        public EntryListViewModel EntryListViewModel
        {
	        get { return entryListViewModel; }
	        private set
	        {
		        entryListViewModel = value;
				RaisePropertyChanged();
	        }
        }

	    public bool IsLoadingEntries
	    {
		    get { return isLoadingEntries; }
		    private set
		    {
			    if (value.Equals(isLoadingEntries)) return;
			    isLoadingEntries = value;
			    RaisePropertyChanged();
		    }
	    }

	    public void Initialize()
	    {
			InitializeAsync().FireAndForget();
	    }

		private async Task InitializeAsync()
		{			
			await InitializeWordListAsync(CompositionFactory, storage);
		}

	    private async Task InitializeWordListAsync(ICompositionFactory compositionFactory, IDataStore storage)
	    {
			IsLoadingEntries = true;

		    using (statusBusyService.Busy())
		    {
			    try
			    {
					await storage.InitializeAsync();
				    var words = await LoadEntries(storage);

				    if (Log.IsDebugEnabled)
					    Log.Debug("Loaded {0} entries from local storage.", words.Count());

				    EntryListViewModel.Entries = words;
			    }
			    finally
			    {
				    if (EntryListViewModel.Entries.Any())
				    {
					    IsLoadingEntries = false;
				    }
			    }
		    }

			try
			{
				await SyncAsync();
			}
			finally
			{
				IsLoadingEntries = false;
				EntryListViewModel.IsInitializationComplete = true;
			}
		    
	    }

	    private static Task<IEnumerable<ClientEntry>> LoadEntries(IDataStore storage)
	    {
		    return storage.LoadEntries(x => !x.IsLearnt);
	    }

	    public async Task SyncAsync(bool force = false)
	    {
		    using (statusBusyService.Busy("Syncing..."))
		    {
			    if (Log.IsDebugEnabled)
				    Log.Debug("Starting synchronization.");

			    // Now when the data from cache is loaded and shown to the user sync with 
			    // the cloud and refresh the data.
			    await storage.EnqueueSync(new OfflineSyncArguments
			    {
				    PurgeCache = force
			    });

			    await RefreshAsync();
		    }
	    }

		private void ForceSync()
		{
			Log.Debug("Forcing synchronization.");

			SyncAsync(true).FireAndForget();
		}

	    private void AddWord()
		{
			View.NavigateToNewWordPage();
		}

		private async void OnEntryCreationRequested(EntryCreationRequestedEvent e)
		{
			var newEntry = new ClientEntry(e.EntryText);

			using (statusBusyService.Busy("Saving..."))
			{
				var addedEntry = await storage.AddEntry(newEntry);

				EntryListViewModel.AddEntry(addedEntry);

				EntryCreationViewModel.Clear();

				EventAggregator.Publish(new EntryCreatedEvent(addedEntry));
			}
		}

	    private async void OnEntryDeletionRequested(EntryDeletionRequestedEvent e)
	    {
		    using (statusBusyService.Busy("Deleting..."))
		    {
			    await storage.DeleteEntry(e.EntryToDelete);

			    EntryListViewModel.DeleteEntryFromUI(e.EntryToDelete);
		    }
	    }

	    public async Task RefreshAsync()
	    {
		    if (Log.IsDebugEnabled)
		    {
				Log.Debug("RefreshAsync");
		    }

		    var words = await LoadEntries(storage);

		    if (Log.IsDebugEnabled)
		    {
				Log.Debug("Loaded {0} entries from local storage.", words.Count());
		    }

			EntryListViewModel.Entries = words;
	    }

		private void SendLogs()
		{
			SendLogsAsync().FireAndForget();
		}

	    private async Task SendLogsAsync()
	    {
			IWinRTLogManager logManager = LogManagerFactory.DefaultLogManager as IWinRTLogManager;

		    if (logManager != null)
		    {
			    await logManager.ShareLogFile(string.Format("Linqua Logs - {0:s} | {1}", DateTime.UtcNow, DeviceInfo.DeviceId), "Linqua compressed log files.");
			}
	    }

		private async void OnEntryIsLearntChanged(EntryIsLearntChangedEvent e)
		{
			storage.UpdateEntry(e.EntryViewModel.Entry).FireAndForget();

			if (e.EntryViewModel.IsLearnt)
			{
				await Observable.Timer(TimeSpan.FromMilliseconds(400));

				Dispatcher.BeginInvoke(new Action(() =>
				{
					EntryListViewModel.DeleteEntryFromUI(e.EntryViewModel.Entry);
				}));
			}
		}
    }
}