using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using MetroLog;

namespace Linqua.ViewModels
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

		[ImportingConstructor]
		public RandomEntryListViewModel([NotNull] IStringResourceManager resourceManager)
	    {
			Guard.NotNull(resourceManager, () => resourceManager);

		    this.resourceManager = resourceManager;

		    RandomEntryViewModels = new ObservableCollection<EntryListItemViewModel>();
		    RandomEntryViewModels.CollectionChanged += OnDisplayEntriesCollectonChanged;

			if (DesignTimeDetection.IsInDesignTool)
			{
				EventAggregator = DesignTimeHelper.EventAggregator;
				RandomEntryViewModels.Add(FakeData.FakeWords.Select(w => CreateListItemViewModel(w)).First());
			}
			
			DeleteEntryCommand = new DelegateCommand<EntryListItemViewModel>(DeleteEntry, CanDeleteEntry);
			ShowNextEntriesCommand = new DelegateCommand(ShowNextEntries, CanShowNextEntries);
			ShowPreviousEntriesCommand = new DelegateCommand(ShowPreviousEntries, CanShowPreviousEntries);
	    }

		public DelegateCommand<EntryListItemViewModel> DeleteEntryCommand { get; private set; }
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

				OnEntriesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				UpdateThereAreNoEntries();

				RaisePropertyChanged();
			}
		}

		public bool AnyEntriesAvailable
		{
			get { return RandomEntryViewModels.Count > 0; }
		}

		public ObservableCollection<EntryListItemViewModel> RandomEntryViewModels { get; private set; }

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

		private void OnEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateThereAreNoEntries();
			UpdatePagingControlsVisibility();
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
			var result = new EntryListItemViewModel(newEntry, justAdded: justAdded);

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
			// Delete items one by one in order to have the list change animations.
			var itemsToDelete = new List<EntryListItemViewModel>(RandomEntryViewModels);

			foreach (var item in itemsToDelete)
			{
				RandomEntryViewModels.Remove(item);
			}
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
			var viewModel = CreateListItemViewModel(newEntry, justAdded: true);

			AddEntry(viewModel);

			return viewModel;
		}

		private void AddEntry(EntryListItemViewModel viewModel)
		{
			if (RandomEntryViewModels.Count == EntriesToDisplayCount)
			{
				RandomEntryViewModels.RemoveAt(0);
			}

			RandomEntryViewModels.Insert(0, viewModel);

			UpdateDisplayedIndexes();
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

		private void DeleteEntry(EntryListItemViewModel obj)
		{
			EventAggregator.Publish(new EntryDeletionRequestedEvent(obj.Entry));
		}

		private bool CanDeleteEntry(EntryListItemViewModel arg)
		{
			return true;
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
			return true;
		}

		private void ShowPreviousEntries()
		{
			throw new NotImplementedException();
		}

		private void OnDisplayEntriesCollectonChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdatePagingControlsVisibility();
			RaisePropertyChanged(() => AnyEntriesAvailable);
		}
    }
}
