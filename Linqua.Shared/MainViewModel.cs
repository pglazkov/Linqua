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
using Linqua.Translation;
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
		private readonly Lazy<ITranslationService> translator;
		private EntryListViewModel entryListViewModel;
		private bool isLoadingEntries;
		private static readonly AsyncLock RefreshLock = new AsyncLock();

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

		public MainViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			Lazy<ITranslationService> translator)
			: this()
		{
			Guard.NotNull(compositionFactory, () => compositionFactory);
			Guard.NotNull(storage, () => storage);
			Guard.NotNull(eventAggregator, () => eventAggregator);
			Guard.NotNull(statusBusyService, () => statusBusyService);
			Guard.NotNull(translator, () => translator);

			this.storage = storage;
			this.eventAggregator = eventAggregator;
			this.statusBusyService = statusBusyService;
			this.translator = translator;

			CompositionFactory = compositionFactory;

			EntryListViewModel = compositionFactory.Create<EntryListViewModel>();
			EntryCreationViewModel = compositionFactory.Create<EntryCreationViewModel>();

			eventAggregator.GetEvent<EntryCreationRequestedEvent>().Subscribe(OnEntryCreationRequested);
			eventAggregator.GetEvent<EntryDeletionRequestedEvent>().Subscribe(OnEntryDeletionRequested);
			eventAggregator.GetEvent<EntryIsLearntChangedEvent>().Subscribe(OnEntryIsLearntChanged);
			eventAggregator.GetEvent<EntryDefinitionChangedEvent>().SubscribeWithAsync(OnEntryDefinitionChangedAsync);
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
			using (await RefreshLock.LockAsync())
			{
				EntryListItemViewModel newEntryViewModel = EntryListViewModel.MoveToTopIfExists(e.EntryText);

				if (newEntryViewModel != null)
				{
					OnEntryAdded(newEntryViewModel.Entry);
				}
				else
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
						await TranslateEntryItemAsync(newEntryViewModel);
					}
				}
			}
		}

		private void OnEntryAdded(ClientEntry addedEntry)
		{
			EntryCreationViewModel.Clear();

			EventAggregator.Publish(new EntryCreatedEvent(addedEntry));
		}

		private async Task TranslateEntryItemAsync(EntryListItemViewModel entryItem)
		{
			using (await RefreshLock.LockAsync())
			{
				entryItem.IsTranslating = true;

				try
				{
					string translation = null;

					try
					{
						if (Log.IsDebugEnabled)
							Log.Debug("Trying to find an existing entry with Text=\"{0}\".", entryItem.Text);

						var existingEntry = await storage.LookupAlreadyExisting(entryItem.Entry);

						if (existingEntry != null && !string.IsNullOrWhiteSpace(existingEntry.Definition))
						{
							if (Log.IsDebugEnabled)
								Log.Debug("Found existing entry with translation: \"{0}\". Entry ID: {1}", existingEntry.Definition, existingEntry.Id);

							translation = existingEntry.Definition;
						}
					}
					catch (Exception ex)
					{
						if (Log.IsErrorEnabled)
							Log.Error("An error occured while trying to find an existing entry.", ex);
					}

					if (string.IsNullOrEmpty(translation))
					{
						if (Log.IsDebugEnabled)
							Log.Debug("Detecting language of \"{0}\"", entryItem.Entry.Text);

						var entryLanguage = await translator.Value.DetectLanguageAsync(entryItem.Entry.Text);

						if (Log.IsDebugEnabled)
							Log.Debug("Detected language: " + entryLanguage);

						if (Log.IsDebugEnabled)
							Log.Debug("Translating \"{0}\" from \"{1}\" to \"{2}\"", entryItem.Entry.Text, entryLanguage, "en");

						translation = await translator.Value.TranslateAsync(entryItem.Entry.Text, entryLanguage, "en");

						if (Log.IsDebugEnabled)
							Log.Debug("Translation: \"{0}\"", translation);
					}

					entryItem.Definition = translation;
				}
				catch (Exception ex)
				{
					if (Log.IsErrorEnabled)
						Log.Error("An error occured while trying to translate an entry.", ex);
				}
				finally
				{
					entryItem.IsTranslating = false;
				}
			}
		}

		private async void OnEntryDeletionRequested(EntryDeletionRequestedEvent e)
		{
			using (await RefreshLock.LockAsync())
			{
				using (statusBusyService.Busy("Deleting..."))
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
			using (await RefreshLock.LockAsync())
			{
				storage.UpdateEntry(e.EntryViewModel.Entry).FireAndForget();

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
			using (await RefreshLock.LockAsync())
			{
				await storage.UpdateEntry(e.EntryViewModel.Entry);
			}
		}
	}
}