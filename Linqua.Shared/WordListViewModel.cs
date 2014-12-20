using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework;
using Linqua.Model;

namespace Linqua
{
    public class WordListViewModel : ViewModelBase
    {
	    public WordListViewModel()
	    {
			if (DesignTimeDetection.IsInDesignTool)
			{
				Words = new ObservableCollection<WordListItemViewModel>(FakeData.FakeWords.Select(w => new WordListItemViewModel(w)));
			}
	    }

	    public WordListViewModel(IEnumerable<Word> words)
	    {
			Guard.NotNull(words, () => words);

			Words = new ObservableCollection<WordListItemViewModel>(words.Select(w => new WordListItemViewModel(w)));
	    }

		public ObservableCollection<WordListItemViewModel> Words { get; private set; }
    }
}