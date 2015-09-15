﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Framework;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Logging;
using Linqua.Persistence;
using Linqua.Service.Models;
using MetroLog;
using Nito.AsyncEx;

namespace Linqua.UI
{
	public class MainViewModel : ViewModelBase
	{
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainViewModel>();

		private readonly IDataStore storage;
		private readonly IEventAggregator eventAggregator;
		private readonly IStatusBusyService statusBusyService;
		private readonly IEntryOperations entryOperations;
		private readonly ILocalSettingsService localSettingsService;
		private readonly IStringResourceManager stringResourceManager;
		private FullEntryListViewModel fullEntryListViewModel;
		private bool isLoadingEntries;
		private bool isEntryEditorVisible;
		private static readonly AsyncLock RefreshLock = new AsyncLock();
		private bool initialized;
		private RandomEntryListViewModel randomEntryListViewModel;
		private bool isStatisticsAvailable;
		private long totalEntriesCount;
		private long notLearnedEntriesCount;
		private bool isLoadingStatistics;
	    private bool isAddingWord;

	    public MainViewModel()
		{
			if (DesignTimeDetection.IsInDesignTool)
			{
				FullEntryListViewModel = new FullEntryListViewModel(FakeData.FakeWords);

				RandomEntryListViewModel = new RandomEntryListViewModel(new StringResourceManager(), new DesignTimeApplicationContoller(), new DefaultRoamingSettingsService())
				{
					Entries = FakeData.FakeWords
				};

				EntryTextEditorViewModel = new EntryTextEditorViewModel(DesignTimeHelper.EventAggregator);
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
			IEntryOperations entryOperations,
			ILocalSettingsService localSettingsService,
			IStringResourceManager stringResourceManager)
			: this()
		{
			Guard.NotNull(compositionFactory, nameof(compositionFactory));
			Guard.NotNull(storage, nameof(storage));
			Guard.NotNull(eventAggregator, nameof(eventAggregator));
			Guard.NotNull(statusBusyService, nameof(statusBusyService));
			Guard.NotNull(entryOperations, nameof(entryOperations));
			Guard.NotNull(localSettingsService, nameof(localSettingsService));
			Guard.NotNull(stringResourceManager, nameof(stringResourceManager));

			this.storage = storage;
			this.eventAggregator = eventAggregator;
			this.statusBusyService = statusBusyService;
			this.entryOperations = entryOperations;
			this.localSettingsService = localSettingsService;
			this.stringResourceManager = stringResourceManager;

			CompositionFactory = compositionFactory;

			FullEntryListViewModel = compositionFactory.Create<FullEntryListViewModel>();
			RandomEntryListViewModel = compositionFactory.Create<RandomEntryListViewModel>();
			EntryTextEditorViewModel = compositionFactory.Create<EntryTextEditorViewModel>();

			eventAggregator.GetEvent<EntryEditingFinishedEvent>().Subscribe(OnEntryEditingFinished);
			eventAggregator.GetEvent<EntryDeletedEvent>().Subscribe(OnEntryDeleted);
			eventAggregator.GetEvent<EntryIsLearntChangedEvent>().Subscribe(OnEntryIsLearntChanged);
			eventAggregator.GetEvent<EntryUpdatedEvent>().SubscribeWithAsync(OnEntryDefinitionChangedAsync);
			eventAggregator.GetEvent<EntryDetailsRequestedEvent>().Subscribe(OnEntryDetailsRequested);
			eventAggregator.GetEvent<EntryQuickEditRequestedEvent>().Subscribe(OnEntryQuickEditRequested);
		}

		public DelegateCommand SendLogsCommand { get; private set; }
		public DelegateCommand AddWordCommand { get; }
		public DelegateCommand SyncCommand { get; private set; }
		public DelegateCommand ToggleShowHideLearnedEntriesCommand { get; private set; }

		public IMainView View { get; set; }

		public EntryTextEditorViewModel EntryTextEditorViewModel { get; }

		public bool ShowLearnedEntries
		{
			get { return localSettingsService.GetValue(LocalSettingsKeys.ShowLearnedEntries, false); }
			set
			{
				localSettingsService.SetValue(LocalSettingsKeys.ShowLearnedEntries, value);
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(ToggleShowHideLearnedEntriesButtonLabel));
			}
		}

		public string ToggleShowHideLearnedEntriesButtonLabel => stringResourceManager.GetString("MainViewModel_ToggleShowHideLearnedEntriesButtonLabel_" + ShowLearnedEntries);

	    public bool IsEntryEditorVisible
		{
			get { return isEntryEditorVisible; }
			set
			{
				if (value.Equals(isEntryEditorVisible)) return;
				isEntryEditorVisible = value;
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

                AddWordCommand.RaiseCanExecuteChanged();
			}
		}

		public bool IsStatisticsAvailable
		{
			get { return isStatisticsAvailable; }
			private set
			{
				if (value == isStatisticsAvailable) return;
				isStatisticsAvailable = value;
				RaisePropertyChanged();
			}
		}

		public bool IsLoadingStatistics
		{
			get { return isLoadingStatistics; }
			private set
			{
				if (value == isLoadingStatistics) return;
				isLoadingStatistics = value;
				RaisePropertyChanged();
			}
		}

		public long TotalEntriesCount
		{
			get { return totalEntriesCount; }
			private set
			{
				if (value == totalEntriesCount) return;
				totalEntriesCount = value;
				RaisePropertyChanged();
			}
		}

		public long NotLearnedEntriesCount
		{
			get { return notLearnedEntriesCount; }
			private set
			{
				if (value == notLearnedEntriesCount) return;
				notLearnedEntriesCount = value;
				RaisePropertyChanged();
			}
		}

		public int PivotSelectedIndex
		{
			get { return localSettingsService.GetValue(LocalSettingsKeys.MainPivotSelectedIndex, 0); }
			set
			{
				if (value == PivotSelectedIndex) return;
				localSettingsService.SetValue(LocalSettingsKeys.MainPivotSelectedIndex, value);
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(IsInFullListMode));
			}
		}

		public bool IsInFullListMode => PivotSelectedIndex == 1;

	    public bool IsAddingWord
	    {
	        get { return isAddingWord; }
	        private set
	        {
	            isAddingWord = value;

                RaisePropertyChanged();

	            Dispatcher.InvokeAsync(() =>
	            {
	                AddWordCommand.RaiseCanExecuteChanged();
	            }).FireAndForget();
	        }
	    }

	    public bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
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
            IEnumerable<ClientEntry> words = null;

            IsLoadingEntries = true;

			using (statusBusyService.Busy())
			{
				using (await RefreshLock.LockAsync())
				{
					try
					{
						await storage.InitializeAsync();
						words = await LoadEntries(storage);

						if (Log.IsDebugEnabled)
							Log.Debug("Loaded {0} entries from local storage.", words.Count());

						UpdateUIWithData(words);

						await UpdateStatistics();
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
				words = await SyncAsync();
			}
			finally
			{
				IsLoadingEntries = false;
				FullEntryListViewModel.IsInitializationComplete = true;
				RandomEntryListViewModel.IsInitializationComplete = true;
			}

		    if (words != null)
		    {
		        await TranslatePendingEntriesAsync(words);
		    }
		}

	    private async Task TranslatePendingEntriesAsync(IEnumerable<ClientEntry> entries)
	    {
	        var pedingTranlsationEntries = entries.Where(x => x.TranslationState == TranslationState.Pending).ToList();

	        using (await RefreshLock.LockAsync())
	        {
	            foreach (var entry in pedingTranlsationEntries)
	            {
	                var vms = FindEntryViewModels(entry);

	                await entryOperations.TranslateEntryItemAsync(entry, vms);

                    await entryOperations.UpdateEntryAsync(entry);
                }
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

		public async Task<IEnumerable<ClientEntry>> SyncAsync(bool force = false)
		{
#if DEBUG
			using (statusBusyService.Busy(CommonBusyType.Syncing))
			{
#endif
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

					return await RefreshInternalAsync();
				}
#if DEBUG
			}
#endif
		}

		private void ForceSync()
		{
			Log.Debug("Forcing synchronization.");

			SyncAsync(true).FireAndForget();
		}

		private bool CanAddWord()
		{
			return !IsEntryEditorVisible && !IsAddingWord && !IsLoadingEntries;
		}

		private void AddWord()
		{
			EventAggregator.Publish(new StopFirstUseTutorialEvent(FirstUseTutorialType.TapToSeeTranslation));

			EntryTextEditorViewModel.Clear();
			IsEntryEditorVisible = true;

			View.FocusEntryCreationView();
		}

		private async void OnEntryEditingFinished(EntryEditingFinishedEvent e)
		{
            IsAddingWord = true;

            try
            {
                var entry = e.Data;

                if (string.IsNullOrEmpty(entry.Id))
                {
                    await AddNewEntryAsync(entry);
                }
                else
                {
                    await UpdateEntryAsync(entry);
                }

                EntryTextEditorViewModel.Clear();
            }
            finally
            {
                IsAddingWord = false;
            }
        }

		private async Task UpdateEntryAsync(ClientEntry entry)
		{
			eventAggregator.Publish(new EntryUpdatedEvent(entry));

			using (await RefreshLock.LockAsync())
			{
				var vms = FindEntryViewModels(entry);

				var translation = await entryOperations.TranslateEntryItemAsync(entry, vms);

				entry.Definition = translation;

				using (statusBusyService.Busy(CommonBusyType.Saving))
				{
					await entryOperations.UpdateEntryAsync(entry);
				}

				await UpdateStatistics();
			}
		}

		private IEnumerable<EntryViewModel> FindEntryViewModels(ClientEntry entry)
		{
			var vm1 = FullEntryListViewModel.Find(entry);
			var vm2 = RandomEntryListViewModel.Find(entry);

			var vms = new List<EntryViewModel>();

			if (vm1 != null)
			{
				vms.Add(vm1);
			}

			if (vm2 != null)
			{
				vms.Add(vm2);
			}

			return vms;
		}

		private async Task AddNewEntryAsync(ClientEntry entry)
		{
			var randomEntryItem = RandomEntryListViewModel.MoveToTopIfExists(entry.Text);
			var fullListItem = FullEntryListViewModel.MoveToTopIfExists(entry.Text);

			var existingItem = fullListItem ?? randomEntryItem;

			if (existingItem != null)
			{
				if (randomEntryItem == null)
				{
					RandomEntryListViewModel.AddEntry(existingItem);
				}

				if (fullListItem == null)
				{
					FullEntryListViewModel.AddEntry(existingItem);
				}

				if (existingItem.IsLearnt)
				{
					existingItem.UnlearnAsync().FireAndForget();
				}

				OnEntryAdded(existingItem.Entry);
			}
			else
			{
                var entryToAdd = entry;

                ClientEntry addedEntry = null;

                EntryListItemViewModel randomListItem;

                using (statusBusyService.Busy(CommonBusyType.Saving))
                {
                    addedEntry = await storage.AddEntry(entryToAdd);

                    fullListItem = FullEntryListViewModel.AddEntry(addedEntry);
                    randomListItem = RandomEntryListViewModel.AddEntry(addedEntry);

                    OnEntryAdded(addedEntry);
                }

                if (string.IsNullOrWhiteSpace(addedEntry.Definition))
                {
                    var translation = await entryOperations.TranslateEntryItemAsync(addedEntry, new[] { randomListItem, fullListItem });

                    addedEntry.Definition = translation;

                    await entryOperations.UpdateEntryAsync(addedEntry);
                }

                await UpdateStatistics();
			}
		}

		private void OnEntryAdded(ClientEntry addedEntry)
		{
			EntryTextEditorViewModel.Clear();
			IsEntryEditorVisible = false;

			EventAggregator.Publish(new EntryCreatedEvent(addedEntry));
		}

		private async void OnEntryDeleted(EntryDeletedEvent e)
		{
			using (statusBusyService.Busy(CommonBusyType.Deleting))
			{
				using (await RefreshLock.LockAsync())
				{
					FullEntryListViewModel.DeleteEntryFromUI(e.DeletedEntry.Entry);
					RandomEntryListViewModel.DeleteEntryFromUI(e.DeletedEntry.Entry);

					await UpdateStatistics();
				}
			}
		}

		public async Task<IEnumerable<ClientEntry>> RefreshAsync()
		{
		    IEnumerable<ClientEntry> entries = null;

		    using (await RefreshLock.LockAsync())
			{
			    entries = await RefreshInternalAsync();
			}

		    if (entries != null)
		    {
		        await TranslatePendingEntriesAsync(entries);
		    }

		    return entries;
		}

	    private async Task<IEnumerable<ClientEntry>> RefreshInternalAsync()
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

			await UpdateStatistics();

		    return words;
		}

		private async Task UpdateStatistics()
		{
			IsLoadingStatistics = true;

			try
			{
				TotalEntriesCount = await storage.GetCount();
				NotLearnedEntriesCount = await storage.GetCount(x => !x.IsLearnt);

				IsStatisticsAvailable = true;
			}
			finally
			{
				IsLoadingStatistics = false;
			}
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
				args.Request.Data.Properties.Title = $"Linqua Logs - {DateTime.UtcNow:s} | {DeviceInfo.DeviceId}";
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
                    // Delay the dissapearing of the entry from the UI (for the sake of visual effect)
					await Observable.Timer(TimeSpan.FromMilliseconds(300));

					await Dispatcher.InvokeAsync(new Action(() =>
					{
					    if (!ShowLearnedEntries)
					    {
					        FullEntryListViewModel.DeleteEntryFromUI(e.EntryViewModel.Entry);
					    }

					    RandomEntryListViewModel.DeleteEntryFromUI(e.EntryViewModel.Entry);
					}));
				}

				await UpdateStatistics();
			}
		}

		private Task OnEntryDefinitionChangedAsync(EntryUpdatedEvent e)
		{
			return Task.FromResult(true);
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

		private void OnEntryQuickEditRequested(EntryQuickEditRequestedEvent e)
		{
			EventAggregator.Publish(new StopFirstUseTutorialEvent(FirstUseTutorialType.TapToSeeTranslation));

			EntryTextEditorViewModel.Clear();
			EntryTextEditorViewModel.Data = e.EntryViewModel.Entry;
			IsEntryEditorVisible = true;

			View.FocusEntryCreationView();
		}

	}
}