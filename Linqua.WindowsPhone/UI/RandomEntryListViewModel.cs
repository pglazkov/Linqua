using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using MetroLog;

namespace Linqua.UI
{
	public class RandomEntryListViewModel : ViewModelBase
    {
		private const int EntriesToDisplayCount = 1;

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<RandomEntryListViewModel>();

		private bool thereAreNoEntries;
		private IList<ClientEntry> entries;
	    private IDictionary<string, ClientEntry> entriesIdDict;
		private bool isInitializationComplete;
		private readonly List<int> displayedIndexes = new List<int>();
		private readonly Random displayEntriesIndexGenerator = new Random((int)DateTime.UtcNow.Ticks);
		private bool canShowNextEntries;
		private readonly IStringResourceManager resourceManager;
		private readonly IEntryOperations entryOperations;
		private readonly IRoamingSettingsService roamingSettings;

		[ImportingConstructor]
		public RandomEntryListViewModel([NotNull] IStringResourceManager resourceManager, [NotNull] IEntryOperations entryOperations, [NotNull] IRoamingSettingsService roamingSettings)
	    {
			Guard.NotNull(resourceManager, nameof(resourceManager));
			Guard.NotNull(entryOperations, nameof(entryOperations));
			Guard.NotNull(roamingSettings, nameof(roamingSettings));

		    this.resourceManager = resourceManager;
			this.entryOperations = entryOperations;
			this.roamingSettings = roamingSettings;

			RandomEntryViewModels = new ObservableCollection<EntryListItemViewModel>();
		    RandomEntryViewModels.CollectionChanged += OnDisplayEntriesCollectonChanged;

			PreviousRandomEntryViewModelsStack = new Stack<List<EntryListItemViewModel>>();

			if (DesignTimeDetection.IsInDesignTool)
			{
				EventAggregator = DesignTimeHelper.EventAggregator;
				RandomEntryViewModels.Add(FakeData.FakeWords.Select(w => CreateListItemViewModel(w)).First());
			}
			
			ShowNextEntriesCommand = new DelegateCommand(ShowNextEntries, () => CanShowNextEntries);
			ShowPreviousEntriesCommand = new DelegateCommand(ShowPreviousEntries, () => CanShowPreviousEntries);
	    }

		public DelegateCommand ShowNextEntriesCommand { get; }
		public DelegateCommand ShowPreviousEntriesCommand { get; }

		public IEnumerable<ClientEntry> Entries
		{
			get { return entries; }
			set
			{
				if (value.ItemsEqual(entries)) return;
				entries = value.ToList();
			    entriesIdDict = entries.ToDictionary(x => x.Id);

				if (Log.IsDebugEnabled)
					Log.Debug("Updateting entries on UI. Entries count: {0}", entries.Count());

				UpdateRandomEntries();

				UpdateThereAreNoEntries();

				RaisePropertyChanged();
			}
		}

		public bool AnyEntriesAvailable => RandomEntryViewModels.Count > 0;

	    public ObservableCollection<EntryListItemViewModel> RandomEntryViewModels { get; }
		private Stack<List<EntryListItemViewModel>> PreviousRandomEntryViewModelsStack { get; }

		public bool IsInitializationComplete
		{
			get { return isInitializationComplete; }
			set
			{
				if (value.Equals(isInitializationComplete)) return;
				isInitializationComplete = value;
				RaisePropertyChanged();
				UpdateThereAreNoEntries();
				UpdateCanShowNextEntries();
			}
		}

	    public bool ThereAreNoEntries
		{
			get { return thereAreNoEntries; }
			private set
			{
				if (value.Equals(thereAreNoEntries)) return;
				thereAreNoEntries = value;
				RaisePropertyChanged();
			}
		}

		public bool CanShowNextEntries
		{
			get { return canShowNextEntries; }
			private set
			{
				if (value.Equals(canShowNextEntries)) return;
				canShowNextEntries = value;
				RaisePropertyChanged();
				ShowNextEntriesCommand.RaiseCanExecuteChanged();
			}
		}

		private void UpdateThereAreNoEntries()
		{
			if (!IsInitializationComplete)
			{
				return;
			}

			ThereAreNoEntries = entries.Count == 0;
		}

		private void UpdateCanShowNextEntries()
		{
			CanShowNextEntries = entries.Count > RandomEntryViewModels.Count;
		}

		private EntryListItemViewModel CreateListItemViewModel(ClientEntry newEntry, bool justAdded = false)
		{
			var result = new EntryListItemViewModel(newEntry, EventAggregator, justAdded: justAdded);

			return result;
		}

		private void UpdateRandomEntries()
		{
			if (entries.Count == 0)
			{
				ClearRandomItems();
				UpdateDisplayedIndexes();
				return;
			}

			if (RandomEntryViewModels.Count < EntriesToDisplayCount)
			{
				var entriesToAdd = EntriesToDisplayCount - RandomEntryViewModels.Count;

				AddRandomDisplayedEntries(entriesToAdd);
			}
			else
			{
				ClearRandomItems();

				var notDisplayedIndexCount = entries.Count - displayedIndexes.Count;

				while (notDisplayedIndexCount < EntriesToDisplayCount && displayedIndexes.Count > 0)
				{
					displayedIndexes.RemoveAt(0);
				}

				AddRandomDisplayedEntries(EntriesToDisplayCount);
			}

			UpdateDisplayedIndexes();
		}

		private void ClearRandomItems()
		{
			var previousRandomEntryViewModels = new List<EntryListItemViewModel>();

			// Delete items one by one in order to have the list change animations.
			var itemsToDelete = new List<EntryListItemViewModel>(RandomEntryViewModels);

			foreach (var item in itemsToDelete)
			{
				RandomEntryViewModels.Remove(item);

			    if (entriesIdDict.ContainsKey(item.Entry.Id))
			    {
			        previousRandomEntryViewModels.Add(item);
			    }
			}

		    if (previousRandomEntryViewModels.Count > 0)
		    {
		        PreviousRandomEntryViewModelsStack.Push(previousRandomEntryViewModels);
		    }

		    UpdateCanShowPreviousEntries();
		}

	    private void UpdateCanShowPreviousEntries()
	    {
	        RaisePropertyChanged(nameof(CanShowPreviousEntries));
	        ShowPreviousEntriesCommand.RaiseCanExecuteChanged();
	    }

	    private void UpdateDisplayedIndexes()
		{
			displayedIndexes.Clear();

			foreach (var vm in RandomEntryViewModels)
			{
				displayedIndexes.Add(entries.IndexOf(vm.Entry));
			}
		}

		private void AddRandomDisplayedEntries(int entriesToAdd)
		{
			for (int i = 0; i < entriesToAdd; i++)
			{
				var index = GenerateNextDisplayIndex();

				if (index == null) break;

				var vm = CreateListItemViewModel(entries[index.Value]);

				vm.JustAdded = false;

				RandomEntryViewModels.Add(vm);

				displayedIndexes.Add(index.Value);
			}
		}


		private int? GenerateNextDisplayIndex()
		{
			Guard.Assert(entries.Count > 0, "entries.Count > 0");

			if (entries.Count <= displayedIndexes.Count)
			{
				return null;
			}

			int result;

			var availableIndexes = new List<int>();

			availableIndexes.AddRange(Enumerable.Range(0, entries.Count).Except(displayedIndexes));

			if (availableIndexes.Count == 0)
			{
				return null;
			}

			var indexOfIndex = displayEntriesIndexGenerator.Next(0, availableIndexes.Count - 1);

			result = availableIndexes[indexOfIndex];

			return result;
		}

		public EntryListItemViewModel AddEntry(ClientEntry newEntry)
		{
			entries.Add(newEntry);
            entriesIdDict.Add(newEntry.Id, newEntry);

			var viewModel = CreateListItemViewModel(newEntry, justAdded: true);

			AddEntry(viewModel);

			return viewModel;
		}

		public void AddEntry(EntryListItemViewModel viewModel)
		{
			if (RandomEntryViewModels.Count == EntriesToDisplayCount)
			{
				RandomEntryViewModels.RemoveAt(0);
			}

			RandomEntryViewModels.Insert(0, viewModel);

			UpdateDisplayedIndexes();

			if (GetIsFirstUseTutorialComplete(FirstUseTutorialType.TapToSeeTranslation) && GetIsFirstUseTutorialComplete(FirstUseTutorialType.FlickToSeeNextRandomWord))
			{
				Observable.Timer(TimeSpan.FromSeconds(1)).ObserveOnDispatcher().Subscribe(_ =>
				{
					viewModel.ShowTranslation();
				});
			}
		}

		public EntryListItemViewModel MoveToTopIfExists(string entryText)
		{
			var existingEntry = entries.FirstOrDefault(x => string.Equals(x.Text, entryText, StringComparison.CurrentCultureIgnoreCase));

			if (existingEntry != null)
			{
				ClearRandomItems();

				UpdateDisplayedIndexes();

				var vm = CreateListItemViewModel(existingEntry, true);

				AddEntry(vm);

				return vm;
			}

			return null;
		}

		public void DeleteEntryFromUI(ClientEntry entryToDelete)
		{
			ClientEntry entry;

		    if (!entriesIdDict.TryGetValue(entryToDelete.Id, out entry))
		    {
		        return;
		    }

			entries.Remove(entry);
		    entriesIdDict.Remove(entry.Id);

		    UpdatePreviousRandomEntryViewModelsStack();

			Observable.Timer(TimeSpan.FromMilliseconds(600)).Subscribe(_ =>
			{
				Dispatcher.InvokeAsync(new Action(UpdateRandomEntries)).FireAndForget();
			});
		}

	    private void UpdatePreviousRandomEntryViewModelsStack()
	    {
            var intermidiateStack = new Stack<List<EntryListItemViewModel>>();

	        while (PreviousRandomEntryViewModelsStack.Count > 0)
	        {
	            var stackEntry = PreviousRandomEntryViewModelsStack.Pop();

	            foreach (var entryVm in new List<EntryListItemViewModel>(stackEntry))
	            {
	                if (!entriesIdDict.ContainsKey(entryVm.Entry.Id))
	                {
	                    stackEntry.Remove(entryVm);
	                }
	            }

	            if (stackEntry.Count > 0)
	            {
	                intermidiateStack.Push(stackEntry);
	            }
	        }

            while (intermidiateStack.Count > 0)
	        {
	            var stackEntry = intermidiateStack.Pop();

	            PreviousRandomEntryViewModelsStack.Push(stackEntry);
	        }
        }

	    private void ShowNextEntries()
		{
			UpdateRandomEntries();
		}

	    public bool CanShowPreviousEntries => PreviousRandomEntryViewModelsStack.Count > 0;

	    private void ShowPreviousEntries()
		{
			var entriesToAdd = PreviousRandomEntryViewModelsStack.Pop();

			RandomEntryViewModels.Clear();

			foreach (var item in entriesToAdd)
			{
				RandomEntryViewModels.Add(item);
			}

			UpdateDisplayedIndexes();

			UpdateCanShowPreviousEntries();
		}

		private void OnDisplayEntriesCollectonChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateThereAreNoEntries();
			UpdateCanShowNextEntries();
			RaisePropertyChanged(nameof(AnyEntriesAvailable));
		}

		[CanBeNull]
		public EntryListItemViewModel Find([NotNull] ClientEntry entry)
		{
			Guard.NotNull(entry, nameof(entry));

			var result = RandomEntryViewModels.FirstOrDefault(x => x.Entry.Id == entry.Id);

			return result;
		}

        public void SetIsFirstUseTutorialComplete(FirstUseTutorialType tutorialType, bool value)
        {
            roamingSettings.SetValue(string.Format(RoamingStorageKeys.IsFirstUseTutorialCompletedKeyTemplate, tutorialType), value);
        }

        public bool GetIsFirstUseTutorialComplete(FirstUseTutorialType tutorialType)
        {
	        return roamingSettings.GetValue<bool>(string.Format(RoamingStorageKeys.IsFirstUseTutorialCompletedKeyTemplate, tutorialType));
        }
    }
}
