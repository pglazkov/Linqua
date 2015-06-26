using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Linq;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using MetroLog;

namespace Linqua.UI
{
    public class FullEntryListViewModel : ViewModelBase
    {
	    private const int EntriesToDisplayCount = 1;

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<FullEntryListViewModel>();

	    private bool thereAreNoEntries;
	    private IEnumerable<ClientEntry> entries;
	    private bool isInitializationComplete;
		private readonly IStringResourceManager resourceManager;
	    private readonly IApplicationController applicationController;
	    private readonly IDictionary<string, EntryListItemTimeGroupViewModel> groupsDictionary = new Dictionary<string, EntryListItemTimeGroupViewModel>();
		private readonly IDictionary<EntryListItemViewModel, EntryListItemTimeGroupViewModel> itemGroupDictionary = new Dictionary<EntryListItemViewModel, EntryListItemTimeGroupViewModel>();

	    [ImportingConstructor]
	    public FullEntryListViewModel([NotNull] IStringResourceManager resourceManager, [NotNull] IApplicationController applicationController)
	    {
			Guard.NotNull(resourceManager, () => resourceManager);
		    Guard.NotNull(applicationController, () => applicationController);

		    this.resourceManager = resourceManager;
		    this.applicationController = applicationController;
		    EntryViewModels = new ObservableCollection<EntryListItemViewModel>();
			EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;
			TimeGroupViewModels = new ObservableCollection<EntryListItemTimeGroupViewModel>();
			TimeGroupViewModels.CollectionChanged += OnTimeGroupsCollectionChanged;

			if (DesignTimeDetection.IsInDesignTool)
			{
				EventAggregator = DesignTimeHelper.EventAggregator;
				EntryViewModels.AddRange(FakeData.FakeWords.Select(w => CreateListItemViewModel(w)));
			}
			
			DeleteEntryCommand = new DelegateCommand<EntryListItemViewModel>(DeleteEntry, CanDeleteEntry);

			EventAggregator.GetEvent<EntryIsLearntChangedEvent>().Subscribe(OnEntryIsLearntChanged);
	    }

	    public FullEntryListViewModel(IEnumerable<ClientEntry> entries)
			: this(new StringResourceManager(), new DesignTimeApplicationContoller())
	    {
			Guard.NotNull(entries, () => entries);

		    Entries = entries;
	    }

	    public DelegateCommand<EntryListItemViewModel> DeleteEntryCommand { get; private set; }
		
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
				EntryViewModels.AddRange(entries.Select(w => CreateListItemViewModel(w)));

			    UpdateTimeGroups();

				OnEntriesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

				UpdateThereAreNoEntries();

			    RaisePropertyChanged();
		    }
	    }

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

	    public bool IsInitializationComplete
	    {
		    get { return isInitializationComplete; }
		    set
		    {
			    if (value.Equals(isInitializationComplete)) return;
			    isInitializationComplete = value;
			    RaisePropertyChanged();
				UpdateThereAreNoEntries();
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
			    if (EntryViewModels.All(x => !x.IsLearnt))
			    {
				    return string.Format(resourceManager.GetString("EntryListView_Header_OneNumber"), EntryViewModels.Count);
			    }
			    else
			    {
					return string.Format(resourceManager.GetString("EntryListView_Header_TwoNumbers"), EntryViewModels.Count(x => !x.IsLearnt), EntryViewModels.Count);
			    }
		    }
	    }

		public EntryListItemViewModel AddEntry(ClientEntry newEntry)
	    {
		    var viewModel = CreateListItemViewModel(newEntry, justAdded: true);

		    AddEntry(viewModel);

			return viewModel;
	    }

	    private EntryListItemViewModel CreateListItemViewModel(ClientEntry newEntry, bool justAdded = false)
	    {
		    var result = new EntryListItemViewModel(newEntry, EventAggregator, justAdded: justAdded);

		    return result;
	    }

	    public void AddEntry(EntryListItemViewModel viewModel)
	    {
		    EntryViewModels.Insert(0, viewModel);

		    AddEntryToGroup(viewModel);
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

		    if (!itemGroupDictionary.ContainsKey(viewModel))
		    {
			    itemGroupDictionary.Add(viewModel, groupViewModel);
		    }
	    }

	    private void DeleteEntry(EntryListItemViewModel obj)
		{
			applicationController.DeleteEntryAsync(obj).FireAndForget();
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

			DeleteEntryFromTimeGroup(entryVm);

			// Move focus to previous or next entry
			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (previousOrNextEntry != null)
				{
					previousOrNextEntry.Focus();
				}
			}));
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

			RaisePropertyChanged(() => TotalCountText);
			RaisePropertyChanged(() => Header);
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
				DeleteEntryFromUI(existingEntry.Entry);
				
				existingEntry.JustAdded = true;

			    AddEntry(existingEntry);

			    return existingEntry;
		    }

		    return null;
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

		private void OnTimeGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

		}

	    private void OnEntryIsLearntChanged(EntryIsLearntChangedEvent e)
	    {
			RaisePropertyChanged(() => TotalCountText);
			RaisePropertyChanged(() => Header);
	    }

	    [CanBeNull]
	    public EntryListItemViewModel Find([NotNull] ClientEntry entry)
	    {
		    Guard.NotNull(entry, () => entry);

		    var result = EntryViewModels.FirstOrDefault(x => x.Entry.Id == entry.Id);

		    return result;
	    }
    }
}