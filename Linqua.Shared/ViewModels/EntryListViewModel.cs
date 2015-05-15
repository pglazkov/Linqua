using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Interop;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using MetroLog;

namespace Linqua
{
    public class EntryListViewModel : ViewModelBase
    {
	    private const int EntriesToDisplayCount = 1;

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<EntryListViewModel>();

	    private bool thereAreNoEntries;
	    private IEnumerable<ClientEntry> entries;
	    private bool isInitializationComplete;
		private readonly List<int> displayedIndexes = new List<int>();
		private readonly Random displayEntriesIndexGenerator = new Random((int)DateTime.UtcNow.Ticks);
	    private bool isPagingControlsVisible;
		private readonly IStringResourceManager resourceManager;
		private readonly IDictionary<string, EntryListItemTimeGroupViewModel> groupsDictionary = new Dictionary<string, EntryListItemTimeGroupViewModel>();
		private readonly IDictionary<EntryListItemViewModel, EntryListItemTimeGroupViewModel> itemGroupDictionary = new Dictionary<EntryListItemViewModel, EntryListItemTimeGroupViewModel>();

	    [ImportingConstructor]
	    public EntryListViewModel([NotNull] IStringResourceManager resourceManager)
	    {
			Guard.NotNull(resourceManager, () => resourceManager);

		    this.resourceManager = resourceManager;
		    EntryViewModels = new ObservableCollection<EntryListItemViewModel>();
			EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

			RandomEntryViewModels = new ObservableCollection<EntryListItemViewModel>();
		    RandomEntryViewModels.CollectionChanged += OnDisplayEntriesCollectonChanged;

			TimeGroupViewModels = new ObservableCollection<EntryListItemTimeGroupViewModel>();
			TimeGroupViewModels.CollectionChanged += OnTimeGroupsCollectionChanged;

			if (DesignTimeDetection.IsInDesignTool)
			{
				EventAggregator = DesignTimeHelper.EventAggregator;
				EntryViewModels.AddRange(FakeData.FakeWords.Select(w => new EntryListItemViewModel(w)));
			}
			
			DeleteEntryCommand = new DelegateCommand<EntryListItemViewModel>(DeleteEntry, CanDeleteEntry);
			ShowNextEntriesCommand = new DelegateCommand(ShowNextEntries, CanShowNextEntries);
			ShowPreviousEntriesCommand = new DelegateCommand(ShowPreviousEntries, CanShowPreviousEntries);
	    }

	    public EntryListViewModel(IEnumerable<ClientEntry> entries)
			: this(new StringResourceManager())
	    {
			Guard.NotNull(entries, () => entries);

		    Entries = entries;
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
			    entries = value;

				if (Log.IsDebugEnabled)
					Log.Debug("Updateting entries on UI. Entries count: {0}", entries.Count());

				EntryViewModels.CollectionChanged -= OnEntriesCollectionChanged;

				EntryViewModels.Clear();
				EntryViewModels.AddRange(entries.Select(w => new EntryListItemViewModel(w)));

			    UpdateTimeGroups();
			    UpdateRandomEntries();

				OnEntriesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

				UpdateThereAreNoEntries();

			    RaisePropertyChanged();
		    }
	    }

	    public ObservableCollection<EntryListItemViewModel> RandomEntryViewModels { get; private set; }
	    public ObservableCollection<EntryListItemViewModel> EntryViewModels { get; private set; }

		public ObservableCollection<EntryListItemTimeGroupViewModel> TimeGroupViewModels { get; private set; }

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
		    }
	    }

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

	    public string TotalCountText
	    {
		    get
		    {
				return string.Format(resourceManager.GetString("EntryListView_TotalCountTemplate"), EntryViewModels.Count);
		    }
	    }

	    public string Header
	    {
		    get
		    {
				return string.Format(resourceManager.GetString("EntryListView_Header"), EntryViewModels.Count);
		    }
	    }

		public EntryListItemViewModel AddEntry(ClientEntry newEntry)
	    {
		    var viewModel = new EntryListItemViewModel(newEntry, justAdded: true);

		    AddEntry(viewModel);

			return viewModel;
	    }

	    private void AddEntry(EntryListItemViewModel viewModel)
	    {
		    EntryViewModels.Insert(0, viewModel);

		    if (RandomEntryViewModels.Count == EntriesToDisplayCount)
		    {
			    RandomEntryViewModels.RemoveAt(0);
		    }

		    AddEntryToGroup(viewModel);

		    RandomEntryViewModels.Insert(0, viewModel);

		    UpdateDisplayedIndexes();
	    }

	    private void AddEntryToGroup(EntryListItemViewModel viewModel)
	    {
		    var group = GetTimeGroupForItem(viewModel);

		    EntryListItemTimeGroupViewModel groupViewModel;

		    if (!groupsDictionary.TryGetValue(group.GroupName, out groupViewModel))
		    {
			    groupViewModel = new EntryListItemTimeGroupViewModel(group.GroupName);

				groupViewModel.Items = new ObservableCollection<EntryListItemViewModel>();

				TimeGroupViewModels.Insert(0, groupViewModel);

				groupsDictionary.Add(group.GroupName, groupViewModel);
		    }
			
			groupViewModel.Items.Insert(0, viewModel);
			itemGroupDictionary.Add(viewModel, groupViewModel);
	    }

	    private void UpdateDisplayedIndexes()
	    {
			displayedIndexes.Clear();

		    foreach (var vm in RandomEntryViewModels)
		    {
			    displayedIndexes.Add(EntryViewModels.IndexOf(vm));
		    }
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
		    var entryVm = EntryViewModels.SingleOrDefault(w => w.Entry.Id == entryToDelete.Id);

		    if (entryVm == null)
		    {
			    return;
		    }

		    var entryIndex = EntryViewModels.IndexOf(entryVm);

		    var previousOrNextEntryIndex = entryIndex > 0
			                                   ? entryIndex - 1
			                                   : (entryIndex < EntryViewModels.Count - 1
				                                      ? entryIndex + 1
				                                      : -1);

		    EntryListItemViewModel previousOrNextEntry = null;

		    if (previousOrNextEntryIndex >= 0)
		    {
			    previousOrNextEntry = EntryViewModels[previousOrNextEntryIndex];
		    }

		    EntryViewModels.RemoveAt(entryIndex);

			RandomEntryViewModels.Remove(entryVm);

			UpdateDisplayedIndexes();

		    DeleteEntryFromTimeGroup(entryVm);

			// Move focus to previous or next entry
			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (previousOrNextEntry != null)
				{
					previousOrNextEntry.Focus();
				}
			}));

		    Observable.Timer(TimeSpan.FromMilliseconds(600)).Subscribe(_ =>
		    {
			    Dispatcher.BeginInvoke(new Action(UpdateRandomEntries));
		    });
	    }

	    private void DeleteEntryFromTimeGroup(EntryListItemViewModel entryVm)
	    {
		    EntryListItemTimeGroupViewModel groupViewModel;

		    if (itemGroupDictionary.TryGetValue(entryVm, out groupViewModel))
		    {
			    groupViewModel.Items.Remove(entryVm);

			    if (groupViewModel.Items.Count == 0)
			    {
				    groupsDictionary.Remove(groupViewModel.GroupName);

				    TimeGroupViewModels.Remove(groupViewModel);
			    }
		    }
	    }

	    private void OnEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateThereAreNoEntries();
			UpdatePagingControlsVisibility();

			RaisePropertyChanged(() => TotalCountText);
			RaisePropertyChanged(() => Header);
		}

	    private void UpdatePagingControlsVisibility()
	    {
		    IsPagingControlsVisible = EntryViewModels.Count > RandomEntryViewModels.Count;
	    }

	    private void UpdateThereAreNoEntries()
		{
			if (!IsInitializationComplete)
			{
				return;
			}

			ThereAreNoEntries = EntryViewModels.Count == 0;
		}

	    public EntryListItemViewModel MoveToTopIfExists(string entryText)
	    {
		    var existingEntry = EntryViewModels.FirstOrDefault(x => string.Equals(x.Text, entryText, StringComparison.CurrentCultureIgnoreCase));

		    if (existingEntry != null)
		    {
			    EntryViewModels.Remove(existingEntry);
			    RandomEntryViewModels.Remove(existingEntry);
			    
				UpdateDisplayedIndexes();

			    existingEntry.JustAdded = true;

			    AddEntry(existingEntry);

			    return existingEntry;
		    }

		    return null;
	    }

		private void UpdateRandomEntries()
		{
			if (EntryViewModels.Count == 0)
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

				var notDisplayedIndexCount = EntryViewModels.Count - displayedIndexes.Count;

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

	    private void AddRandomDisplayedEntries(int entriesToAdd)
	    {
		    for (int i = 0; i < entriesToAdd; i++)
		    {
			    var index = GenerateNextDisplayIndex();

			    if (index == null) break;

			    var vm = EntryViewModels[index.Value];

			    vm.JustAdded = false;

			    RandomEntryViewModels.Add(vm);

			    displayedIndexes.Add(index.Value);
		    }
	    }

	    private int? GenerateNextDisplayIndex()
	    {
			Guard.Assert(EntryViewModels.Count > 0, "EntryViewModels.Count > 0");

		    if (EntryViewModels.Count <= displayedIndexes.Count)
		    {
			    return null;
		    }

		    int result;

			var availableIndexes = new List<int>();

			availableIndexes.AddRange(Enumerable.Range(0, EntryViewModels.Count).Except(displayedIndexes));

		    if (availableIndexes.Count == 0)
		    {
			    return null;
		    }

		    var indexOfIndex = displayEntriesIndexGenerator.Next(0, availableIndexes.Count - 1);

		    result = availableIndexes[indexOfIndex];

		    return result;
	    }

		private void UpdateTimeGroups()
		{
			TimeGroupViewModels.CollectionChanged -= OnTimeGroupsCollectionChanged;

			TimeGroupViewModels.Clear();
			groupsDictionary.Clear();
			itemGroupDictionary.Clear();

			var entriesWithGroups = EntryViewModels.Select(x => new
			{
				TimeGroup = GetTimeGroupForItem(x),
				EntryVm = x
			}).ToList();

			var groupedItems = entriesWithGroups.GroupBy(i => i.TimeGroup).OrderByDescending(g => g.Key.OrderIndex).ToList();

			foreach (var group in groupedItems)
			{
				var groupName = @group.Key.GroupName;

				var groupVm = new EntryListItemTimeGroupViewModel(groupName);

				var sortedItems = @group.OrderByDescending(i => i.EntryVm.DateAdded).Select(x => x.EntryVm);

				groupVm.Items = new ObservableCollection<EntryListItemViewModel>(sortedItems);

				TimeGroupViewModels.Add(groupVm);

				foreach (var entry in groupVm.Items)
				{
					itemGroupDictionary.Add(entry, groupVm);
				}

				groupsDictionary.Add(groupName, groupVm);
			}

			TimeGroupViewModels.CollectionChanged += OnTimeGroupsCollectionChanged;
		}

		private static DateTimeGroupInfo GetTimeGroupForItem(EntryListItemViewModel x)
	    {
		    return DateTimeGrouping.GetGroup(x.DateAdded, DateTime.Now);
	    }

	    private bool CanShowNextEntries()
		{
			return true;
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
		}

		private void OnTimeGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

		}
    }
}