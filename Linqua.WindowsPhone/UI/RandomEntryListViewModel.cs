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
using MetroLog;

namespace Linqua.UI
{
	public class RandomEntryListViewModel : ViewModelBase
    {
		private const int EntriesToDisplayCount = 1;

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<FullEntryListViewModel>();

		private bool thereAreNoEntries;
		private IList<ClientEntry> entries;
		private bool isInitializationComplete;
		private readonly List<int> displayedIndexes = new List<int>();
		private readonly Random displayEntriesIndexGenerator = new Random((int)DateTime.UtcNow.Ticks);
		private bool isPagingControlsVisible;
		private readonly IStringResourceManager resourceManager;
		private readonly IEntryOperations entryOperations;
		private readonly IRoamingSettingsService roamingSettings;

		[ImportingConstructor]
		public RandomEntryListViewModel([NotNull] IStringResourceManager resourceManager, [NotNull] IEntryOperations entryOperations, [NotNull] IRoamingSettingsService roamingSettings)
	    {
			Guard.NotNull(resourceManager, () => resourceManager);
			Guard.NotNull(entryOperations, () => entryOperations);
			Guard.NotNull(roamingSettings, () => roamingSettings);

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
			
			ShowNextEntriesCommand = new DelegateCommand(ShowNextEntries, CanShowNextEntries);
			ShowPreviousEntriesCommand = new DelegateCommand(ShowPreviousEntries, CanShowPreviousEntries);
	    }

		public DelegateCommand ShowNextEntriesCommand { get; private set; }
		public DelegateCommand ShowPreviousEntriesCommand { get; private set; }

		public IEnumerable<ClientEntry> Entries
		{
			get { return entries; }
			set
			{
				if (value.ItemsEqual(entries)) return;
				entries = value.ToList();

				if (Log.IsDebugEnabled)
					Log.Debug("Updateting entries on UI. Entries count: {0}", entries.Count());

				UpdateRandomEntries();

				UpdateThereAreNoEntries();

				RaisePropertyChanged();
			}
		}

		public bool AnyEntriesAvailable
		{
			get { return RandomEntryViewModels.Count > 0; }
		}

		public ObservableCollection<EntryListItemViewModel> RandomEntryViewModels { get; private set; }
		private Stack<List<EntryListItemViewModel>> PreviousRandomEntryViewModelsStack { get; set; }

		public bool IsInitializationComplete
		{
			get { return isInitializationComplete; }
			set
			{
				if (value.Equals(isInitializationComplete)) return;
				isInitializationComplete = value;
				RaisePropertyChanged();
				UpdateThereAreNoEntries();
				UpdatePagingControlsVisibility();
			}
		}

		public bool IsFirstUseTutorialComplete
		{
			get { return roamingSettings.GetValue<bool>(RoamingStorageKeys.IsRandomEntryUITutorialCompletedKey); }
			set { roamingSettings.SetValue(RoamingStorageKeys.IsRandomEntryUITutorialCompletedKey, value); }
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

		public bool IsPagingControlsVisible
		{
			get { return isPagingControlsVisible; }
			private set
			{
				if (value.Equals(isPagingControlsVisible)) return;
				isPagingControlsVisible = value;
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

		private void UpdatePagingControlsVisibility()
		{
			IsPagingControlsVisible = entries.Count > RandomEntryViewModels.Count;
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
				previousRandomEntryViewModels.Add(item);
			}

			PreviousRandomEntryViewModelsStack.Push(previousRandomEntryViewModels);
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

			if (IsFirstUseTutorialComplete)
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
			var entry = entries.SingleOrDefault(w => w.Id == entryToDelete.Id);

			if (entry == null)
			{
				return;
			}

			entries.Remove(entry);

			Observable.Timer(TimeSpan.FromMilliseconds(600)).Subscribe(_ =>
			{
				Dispatcher.BeginInvoke(new Action(UpdateRandomEntries));
			});
		}

		private bool CanShowNextEntries()
		{
			return IsPagingControlsVisible;
		}

		private void ShowNextEntries()
		{
			UpdateRandomEntries();
		}

		private bool CanShowPreviousEntries()
		{
			return PreviousRandomEntryViewModelsStack.Count > 0;
		}

		private void ShowPreviousEntries()
		{
			var entriesToAdd = PreviousRandomEntryViewModelsStack.Pop();

			RandomEntryViewModels.Clear();

			foreach (var item in entriesToAdd)
			{
				RandomEntryViewModels.Add(item);
			}

			UpdateDisplayedIndexes();

			ShowPreviousEntriesCommand.RaiseCanExecuteChanged();
		}

		private void OnDisplayEntriesCollectonChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateThereAreNoEntries();
			UpdatePagingControlsVisibility();

			UpdatePagingControlsVisibility();
			RaisePropertyChanged(() => AnyEntriesAvailable);
		}

		[CanBeNull]
		public EntryListItemViewModel Find([NotNull] ClientEntry entry)
		{
			Guard.NotNull(entry, () => entry);

			var result = RandomEntryViewModels.FirstOrDefault(x => x.Entry.Id == entry.Id);

			return result;
		}
    }
}
