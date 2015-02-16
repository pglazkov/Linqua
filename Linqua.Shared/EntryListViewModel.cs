﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Linq;
using Framework;
using Linqua.DataObjects;
using Linqua.Events;
using MetroLog;

namespace Linqua
{
    public class EntryListViewModel : ViewModelBase
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<EntryListViewModel>();

	    private bool thereAreNoEntries;
	    private IEnumerable<ClientEntry> entries;
	    private bool isInitializationComplete;

	    public EntryListViewModel()
	    {
		    EntryViewModels = new ObservableCollection<EntryListItemViewModel>();
			EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

			if (DesignTimeDetection.IsInDesignTool)
			{
				EventAggregator = DesignTimeHelper.EventAggregator;
				EntryViewModels.AddRange(FakeData.FakeWords.Select(w => new EntryListItemViewModel(w)));
			}

			DeleteEntryCommand = new DelegateCommand<EntryListItemViewModel>(DeleteEntry, CanDeleteEntry);
	    }

	    public EntryListViewModel(IEnumerable<ClientEntry> entries)
			: this()
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
				EntryViewModels.AddRange(entries.Select(w => new EntryListItemViewModel(w)));

				EntryViewModels.CollectionChanged += OnEntriesCollectionChanged;

				UpdateThereAreNoEntries();

			    RaisePropertyChanged();
		    }
	    }

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

	    public void AddEntry(ClientEntry newEntry)
	    {
		    EntryViewModels.Add(new EntryListItemViewModel(newEntry, justAdded: true));
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

		    EntryViewModels.Remove(entryVm);
	    }

		private void OnEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateThereAreNoEntries();
		}

		private void UpdateThereAreNoEntries()
		{
			if (!IsInitializationComplete)
			{
				return;
			}

			ThereAreNoEntries = EntryViewModels.Count == 0;
		}
    }
}