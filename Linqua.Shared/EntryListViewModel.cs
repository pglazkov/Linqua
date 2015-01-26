using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework;
using Linqua.DataObjects;

namespace Linqua
{
    public class EntryListViewModel : ViewModelBase
    {
	    public EntryListViewModel()
	    {
			if (DesignTimeDetection.IsInDesignTool)
			{
				Words = new ObservableCollection<EntryListItemViewModel>(FakeData.FakeWords.Select(w => new EntryListItemViewModel(w)));
			}
	    }

	    public EntryListViewModel(IEnumerable<ClientEntry> words)
	    {
			Guard.NotNull(words, () => words);

			Words = new ObservableCollection<EntryListItemViewModel>(words.Select(w => new EntryListItemViewModel(w)));
	    }

		public ObservableCollection<EntryListItemViewModel> Words { get; private set; }
    }
}