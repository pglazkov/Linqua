using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Interop;
using Framework;
using Linqua.DataObjects;
using Linqua.Events;
using MetroLog;

namespace Linqua
{
    public class EntryListViewModel : ViewModelBase
    {
	    private const int EntriesToDisplayCount = 3;

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<EntryListViewModel>();

	    private bool thereAreNoEntries;
	    private IEnumerable<ClientEntry> entries;
	    private bool isInitializationComplete;
		private readonly List<int> displayedIndexes = new List<int>();
		private readonly Random displayEntriesIndexGenerator = new Random((int)DateTime.UtcNow.Ticks);
	    private bool isPagingControlsVisible;

	    public EntryListViewModel()
	    {
		    EntryViewModels = new ObservableCollection<EntryListItemViewModel>();
			EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

			DisplayEntryViewModels = new ObservableCollection<EntryListItemViewModel>();
		    DisplayEntryViewModels.CollectionChanged += OnDisplayEntriesCollectonChanged;

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
			: this()
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

			    UpdateDisplayedEntries();
				UpdatePagingControlsVisibility();

				EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

				UpdateThereAreNoEntries();

			    RaisePropertyChanged();
		    }
	    }

	    public ObservableCollection<EntryListItemViewModel> DisplayEntryViewModels { get; private set; }
	    public ObservableCollection<EntryListItemViewModel> EntryViewModels { get; private set; }

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

		public EntryListItemViewModel AddEntry(ClientEntry newEntry)
	    {
		    var viewModel = new EntryListItemViewModel(newEntry, justAdded: true);

		    AddEntry(viewModel);

			return viewModel;
	    }

	    private void AddEntry(EntryListItemViewModel viewModel)
	    {
		    EntryViewModels.Insert(0, viewModel);
		    DisplayEntryViewModels.Insert(0, viewModel);
		    UpdateDisplayedIndexes();
	    }

	    private void UpdateDisplayedIndexes()
	    {
			displayedIndexes.Clear();

		    foreach (var vm in DisplayEntryViewModels)
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

			Guard.Assert(entryVm != null, "entryVm != null");

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

			if (displayedIndexes.Contains(entryIndex))
			{
				displayedIndexes.Remove(entryIndex);
				DisplayEntryViewModels.Remove(entryVm);

				UpdateDisplayedIndexes();
			}

			// Move focus to previous or next entry
			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (previousOrNextEntry != null)
				{
					previousOrNextEntry.Focus();
				}
			}));
	    }

		private void OnEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateThereAreNoEntries();
			UpdatePagingControlsVisibility();
		}

	    private void UpdatePagingControlsVisibility()
	    {
		    IsPagingControlsVisible = EntryViewModels.Count > DisplayEntryViewModels.Count;
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
			    DisplayEntryViewModels.Remove(existingEntry);
			    
				UpdateDisplayedIndexes();

			    existingEntry.JustAdded = true;

			    AddEntry(existingEntry);

			    return existingEntry;
		    }

		    return null;
	    }

		private void UpdateDisplayedEntries()
		{
			if (EntryViewModels.Count == 0)
			{
				DisplayEntryViewModels.Clear();
				UpdateDisplayedIndexes();
				return;
			}

			if (DisplayEntryViewModels.Count < EntriesToDisplayCount)
			{
				var entriesToAdd = EntriesToDisplayCount - DisplayEntryViewModels.Count;

				for (int i = 0; i < entriesToAdd; i++)
				{
					var index = GenerateNextDisplayIndex();

					if (index == null) break;

					DisplayEntryViewModels.Add(EntryViewModels[index.Value]);
					displayedIndexes.Add(index.Value);
				}
			}
			else
			{
				DisplayEntryViewModels.Clear();

				var notDisplayedIndexCount = EntryViewModels.Count - displayedIndexes.Count;

				while (notDisplayedIndexCount < EntriesToDisplayCount && displayedIndexes.Count > 0)
				{
					displayedIndexes.RemoveAt(0);
				}
				
				for (int i = 0; i < EntriesToDisplayCount; i++)
				{
					var index = GenerateNextDisplayIndex();

					if (index == null) break;

					DisplayEntryViewModels.Add(EntryViewModels[index.Value]);
					displayedIndexes.Add(index.Value);
				}
			}

			UpdateDisplayedIndexes();
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

		private bool CanShowNextEntries()
		{
			return true;
		}

		private void ShowNextEntries()
		{
			UpdateDisplayedEntries();
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

		}
    }
}