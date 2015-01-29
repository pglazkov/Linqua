using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Framework;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua
{
    public class EntryListViewModel : ViewModelBase
    {
	    public EntryListViewModel()
	    {
			if (DesignTimeDetection.IsInDesignTool)
			{
				EventAggregator = DesignTimeHelper.EventAggregator;
				Entries = new ObservableCollection<EntryListItemViewModel>(FakeData.FakeWords.Select(w => new EntryListItemViewModel(w)));
			}

			DeleteEntryCommand = new DelegateCommand<EntryListItemViewModel>(DeleteEntry, CanDeleteEntry);
	    }

	    public EntryListViewModel(IEnumerable<ClientEntry> entries)
			: this()
	    {
			Guard.NotNull(entries, () => entries);

			Entries = new ObservableCollection<EntryListItemViewModel>(entries.Select(w => new EntryListItemViewModel(w)));
	    }

		public DelegateCommand<EntryListItemViewModel> DeleteEntryCommand { get; private set; }

		public ObservableCollection<EntryListItemViewModel> Entries { get; private set; }

	    public void AddEntry(ClientEntry newEntry)
	    {
		    Entries.Add(new EntryListItemViewModel(newEntry));
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
		    var entryVm = Entries.SingleOrDefault(w => w.Entry.Id == entryToDelete.Id);

			Guard.Assert(entryVm != null, "entryVm != null");

		    Entries.Remove(entryVm);
	    }
    }
}