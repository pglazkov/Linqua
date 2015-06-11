using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Framework;
using Framework.PlatformServices;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Logging;
using Linqua.Persistence;
using Linqua.ViewModels;
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
		private readonly ILocalSettingsService localSettingsService;
		private readonly IStringResourceManager stringResourceManager;
		private FullEntryListViewModel fullEntryListViewModel;
		private bool isLoadingEntries;
		private bool isEntryCreationViewVisible;
		private static readonly AsyncLock RefreshLock = new AsyncLock();
		private bool initialized;
		private RandomEntryListViewModel randomEntryListViewModel;

		public MainViewModel()
		{
			if (DesignTimeDetection.IsInDesignTool)
			{
				FullEntryListViewModel = new FullEntryListViewModel(FakeData.FakeWords);

				RandomEntryListViewModel = new RandomEntryListViewModel(new StringResourceManager())
				{
					Entries = FakeData.FakeWords
				};

				EntryCreationViewModel = new EntryCreationViewModel(DesignTimeHelper.EventAggregator);
			}

			SyncCommand = new DelegateCommand(ForceSync);
			SendLogsCommand = new DelegateCommand(SendLogs);
			AddWordCommand = new DelegateCommand(AddWord, CanAddWord);
			ToggleShowHideLearnedEntriesCommand = new DelegateCommand(ToggleShowHideLearnedEntries);
		}

		public MainViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			IApplicationController applicationController,
			ILocalSettingsService localSettingsService,
			IStringResourceManager stringResourceManager)
			: this()
		{
			Guard.NotNull(compositionFactory, () => compositionFactory);
			Guard.NotNull(storage, () => storage);
			Guard.NotNull(eventAggregator, () => eventAggregator);
			Guard.NotNull(statusBusyService, () => statusBusyService);
			Guard.NotNull(applicationController, () => applicationController);
			Guard.NotNull(localSettingsService, () => localSettingsService);
			Guard.NotNull(stringResourceManager, () => stringResourceManager);

			this.storage = storage;
			this.eventAggregator = eventAggregator;
			this.statusBusyService = statusBusyService;
			this.applicationController = applicationController;
			this.localSettingsService = localSettingsService;
			this.stringResourceManager = stringResourceManager;

			CompositionFactory = compositionFactory;

			FullEntryListViewModel = compositionFactory.Create<FullEntryListViewModel>();
			RandomEntryListViewModel = compositionFactory.Create<RandomEntryListViewModel>();
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
		public DelegateCommand ToggleShowHideLearnedEntriesCommand { get; private set; }

		public IMainView View { get; set; }

		public EntryCreationViewModel EntryCreationViewModel { get; private set; }

		public bool ShowLearnedEntries
		{
			get { return localSettingsService.GetValue(LocalSettingsKeys.ShowLearnedEntries, false); }
			set
			{
				localSettingsService.SetValue(LocalSettingsKeys.ShowLearnedEntries, value);
				RaisePropertyChanged();
				RaisePropertyChanged(() => ToggleShowHideLearnedEntriesButtonLabel);
			}
		}

		public string ToggleShowHideLearnedEntriesButtonLabel
		{
			get { return stringResourceManager.GetString("MainViewModel_ToggleShowHideLearnedEntriesButtonLabel_" + ShowLearnedEntries); }
		}

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

		public FullEntryListViewModel FullEntryListViewModel
		{
			get { return fullEntryListViewModel; }
			private set
			{
				fullEntryListViewModel = value;
				RaisePropertyChanged();
			}
		}

		public RandomEntryListViewModel RandomEntryListViewModel
		{
			get { return randomEntryListViewModel; }
			private set
			{
				if (Equals(value, randomEntryListViewModel)) return;
				randomEntryListViewModel = value;
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

			await InitializeWordListAsync(storage);

			initialized = true;
		}

		private async Task InitializeWordListAsync(IDataStore storage)
		{
			IsLoadingEntries = true;

			using (statusBusyService.Busy())
			{
				using (await RefreshLock.LockAsync())
				{
					IEnumerable<ClientEntry> words = null;

					try
					{
						await storage.InitializeAsync();
						words = await LoadEntries(storage);

						if (Log.IsDebugEnabled)
							Log.Debug("Loaded {0} entries from local storage.", words.Count());

						UpdateUIWithData(words);
					}
					finally
					{
						if (words.Any())
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
				FullEntryListViewModel.IsInitializationComplete = true;
				RandomEntryListViewModel.IsInitializationComplete = true;
			}

		}

		private void UpdateUIWithData(IEnumerable<ClientEntry> words)
		{
			var allEntries = words.ToList();

			FullEntryListViewModel.Entries = allEntries;
			RandomEntryListViewModel.Entries = allEntries.Where(x => !x.IsLearnt).ToList();
		}

		private Task<IEnumerable<ClientEntry>> LoadEntries(IDataStore storage)
		{
			if (ShowLearnedEntries)
			{
				return storage.LoadEntries();
			}
			else
			{
				return storage.LoadEntries(x => !x.IsLearnt);
			}
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
			EventAggregator.Publish(new StopFirstUseTutorialEvent());

			IsEntryCreationViewVisible = true;
			View.FocusEntryCreationView();
		}

		private async void OnEntryCreationRequested(EntryCreationRequestedEvent e)
		{
			RandomEntryListViewModel.MoveToTopIfExists(e.EntryText);

			var fullListItem = FullEntryListViewModel.MoveToTopIfExists(e.EntryText);

			if (fullListItem != null)
			{
				OnEntryAdded(fullListItem.Entry);
			}
			else
			{
				using (await RefreshLock.LockAsync())
				{
					var entryToAdd = new ClientEntry(e.EntryText);

					ClientEntry addedEntry = null;

					EntryListItemViewModel randomListItem;

					using (statusBusyService.Busy("Saving..."))
					{
						addedEntry = await storage.AddEntry(entryToAdd);

						fullListItem = FullEntryListViewModel.AddEntry(addedEntry);
						randomListItem = RandomEntryListViewModel.AddEntry(addedEntry);

						OnEntryAdded(addedEntry);
					}
					
					if (string.IsNullOrWhiteSpace(addedEntry.Definition))
					{
						await applicationController.TranslateEntryItemAsync(addedEntry, new[] { randomListItem, fullListItem });
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

					FullEntryListViewModel.DeleteEntryFromUI(e.EntryToDelete);
					RandomEntryListViewModel.DeleteEntryFromUI(e.EntryToDelete);
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

			UpdateUIWithData(words);
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
						FullEntryListViewModel.DeleteEntryFromUI(e.EntryViewModel.Entry);
						RandomEntryListViewModel.DeleteEntryFromUI(e.EntryViewModel.Entry);
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

		private void ToggleShowHideLearnedEntries()
		{
			ShowLearnedEntries = !ShowLearnedEntries;

			RefreshWithBusyNotificationAsync().FireAndForget();
		}

		private async Task RefreshWithBusyNotificationAsync()
		{
			using (statusBusyService.Busy())
			{
				await RefreshAsync();
			}
		}
	}
}