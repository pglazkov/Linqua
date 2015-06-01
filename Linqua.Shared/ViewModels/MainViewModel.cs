using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Framework;
using Framework.PlatformServices;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Logging;
using Linqua.Persistence;
using MetroLog;
using Nito.AsyncEx;

namespace Linqua
{
	public class MainViewModel : ViewModelBase
	{
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainViewModel>();

		private readonly IDataStore storage;
		private readonly IEventAggregator eventAggregator;
		private readonly IStatusBusyService statusBusyService;
		private readonly IApplicationController applicationController;
		private EntryListViewModel entryListViewModel;
		private bool isLoadingEntries;
		private bool isEntryCreationViewVisible;
		private static readonly AsyncLock RefreshLock = new AsyncLock();
		private bool initialized;

		public MainViewModel()
		{
			if (DesignTimeDetection.IsInDesignTool)
			{
				EntryListViewModel = new EntryListViewModel(FakeData.FakeWords);
				EntryCreationViewModel = new EntryCreationViewModel(DesignTimeHelper.EventAggregator);
			}

			SyncCommand = new DelegateCommand(ForceSync);
			SendLogsCommand = new DelegateCommand(SendLogs);
			AddWordCommand = new DelegateCommand(AddWord, CanAddWord);
		}

		public MainViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			IApplicationController applicationController)
			: this()
		{
			Guard.NotNull(compositionFactory, () => compositionFactory);
			Guard.NotNull(storage, () => storage);
			Guard.NotNull(eventAggregator, () => eventAggregator);
			Guard.NotNull(statusBusyService, () => statusBusyService);
			Guard.NotNull(applicationController, () => applicationController);

			this.storage = storage;
			this.eventAggregator = eventAggregator;
			this.statusBusyService = statusBusyService;
			this.applicationController = applicationController;

			CompositionFactory = compositionFactory;

			EntryListViewModel = compositionFactory.Create<EntryListViewModel>();
			EntryCreationViewModel = compositionFactory.Create<EntryCreationViewModel>();

			eventAggregator.GetEvent<EntryCreationRequestedEvent>().Subscribe(OnEntryCreationRequested);
			eventAggregator.GetEvent<EntryDeletionRequestedEvent>().Subscribe(OnEntryDeletionRequested);
			eventAggregator.GetEvent<EntryIsLearntChangedEvent>().Subscribe(OnEntryIsLearntChanged);
			eventAggregator.GetEvent<EntryDefinitionChangedEvent>().SubscribeWithAsync(OnEntryDefinitionChangedAsync);
			eventAggregator.GetEvent<EntryDetailsRequestedEvent>().Subscribe(OnEntryDetailsRequested);
		}

		public DelegateCommand SendLogsCommand { get; private set; }
		public DelegateCommand AddWordCommand { get; private set; }
		public DelegateCommand SyncCommand { get; private set; }

		public IMainView View { get; set; }

		public EntryCreationViewModel EntryCreationViewModel { get; private set; }

		public bool IsEntryCreationViewVisible
		{
			get { return isEntryCreationViewVisible; }
			set
			{
				if (value.Equals(isEntryCreationViewVisible)) return;
				isEntryCreationViewVisible = value;
				RaisePropertyChanged();
				AddWordCommand.RaiseCanExecuteChanged();
			}
		}

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
			if (initialized)
			{
				return;
			}

			await InitializeWordListAsync(CompositionFactory, storage);

			initialized = true;
		}

		private async Task InitializeWordListAsync(ICompositionFactory compositionFactory, IDataStore storage)
		{
			IsLoadingEntries = true;

			using (statusBusyService.Busy())
			{
				using (await RefreshLock.LockAsync())
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
				using (await RefreshLock.LockAsync())
				{
					if (Log.IsDebugEnabled)
						Log.Debug("Starting synchronization.");

					// Now when the data from cache is loaded and shown to the user sync with 
					// the cloud and refresh the data.
					await storage.EnqueueSync(new OfflineSyncArguments
					{
						PurgeCache = force
					});

					await RefreshInternalAsync();
				}
			}
		}

		private void ForceSync()
		{
			Log.Debug("Forcing synchronization.");

			SyncAsync(true).FireAndForget();
		}

		private bool CanAddWord()
		{
			return !IsEntryCreationViewVisible;
		}

		private void AddWord()
		{
			IsEntryCreationViewVisible = true;
			View.FocusEntryCreationView();
		}

		private async void OnEntryCreationRequested(EntryCreationRequestedEvent e)
		{
			EntryListItemViewModel newEntryViewModel = EntryListViewModel.MoveToTopIfExists(e.EntryText);

			if (newEntryViewModel != null)
			{
				OnEntryAdded(newEntryViewModel.Entry);
			}
			else
			{
				using (await RefreshLock.LockAsync())
				{
					var newEntry = new ClientEntry(e.EntryText);

					using (statusBusyService.Busy("Saving..."))
					{
						var addedEntry = await storage.AddEntry(newEntry);

						newEntryViewModel = EntryListViewModel.AddEntry(addedEntry);

						OnEntryAdded(addedEntry);
					}

					if (newEntryViewModel != null && string.IsNullOrWhiteSpace(newEntryViewModel.Definition))
					{
						await applicationController.TranslateEntryItemAsync(newEntryViewModel);
					}
				}
			}
		}

		private void OnEntryAdded(ClientEntry addedEntry)
		{
			EntryCreationViewModel.Clear();
			IsEntryCreationViewVisible = false;

			EventAggregator.Publish(new EntryCreatedEvent(addedEntry));
		}

		private async void OnEntryDeletionRequested(EntryDeletionRequestedEvent e)
		{
			using (statusBusyService.Busy("Deleting..."))
			{
				using (await RefreshLock.LockAsync())
				{
					await storage.DeleteEntry(e.EntryToDelete);

					EntryListViewModel.DeleteEntryFromUI(e.EntryToDelete);
				}
			}
		}

		public async Task RefreshAsync()
		{
			using (await RefreshLock.LockAsync())
			{
				await RefreshInternalAsync();
			}
		}

		private async Task RefreshInternalAsync()
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
			try
			{
				var dtm = DataTransferManager.GetForCurrentView();

				dtm.DataRequested += OnLogFilesShareDataRequested;

				DataTransferManager.ShowShareUI();
			}
			catch (Exception ex)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Unexpected exception occured while trying to share the log files.", ex);
			}
		}

		private static async void OnLogFilesShareDataRequested(object sender, DataRequestedEventArgs args)
		{
			if (Log.IsDebugEnabled)
				Log.Debug("Prepearing compressed logs to share.");

			if (Log.IsDebugEnabled)
				Log.Debug("Deferral deadline is: {0}", args.Request.Deadline);

			Stopwatch sw = new Stopwatch();
			
			var deferral = args.Request.GetDeferral();

			sw.Start();

			try
			{
				args.Request.Data.Properties.Title = string.Format("Linqua Logs - {0:s} | {1}", DateTime.UtcNow, DeviceInfo.DeviceId);
				args.Request.Data.Properties.Description = "Linqua compressed log files.";

				var file = await FileStreamingTarget.Instance.GetCompressedLogFile();

				args.Request.Data.SetStorageItems(new[] { file });
			}
			catch (Exception ex)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Unexpected exception occured while trying to share the log files.", ex);
			}
			finally
			{
				sw.Stop();

				if (Log.IsDebugEnabled)
					Log.Debug("Compressed log file obtained at {0}", DateTime.Now);

				deferral.Complete();

				var dtm = DataTransferManager.GetForCurrentView();
				dtm.DataRequested -= OnLogFilesShareDataRequested;
			}
		}

		private async void OnEntryIsLearntChanged(EntryIsLearntChangedEvent e)
		{
			using (await RefreshLock.LockAsync())
			{
				if (e.EntryViewModel.IsLearnt)
				{
					await Observable.Timer(TimeSpan.FromMilliseconds(300));

					Dispatcher.BeginInvoke(new Action(() =>
					{
						EntryListViewModel.DeleteEntryFromUI(e.EntryViewModel.Entry);
					}));
				}
			}
		}

		private async Task OnEntryDefinitionChangedAsync(EntryDefinitionChangedEvent e)
		{
			
		}

		private void OnEntryDetailsRequested(EntryDetailsRequestedEvent e)
		{
			View.NavigateToEntryDetails(e.EntryId);
		}
	}
}