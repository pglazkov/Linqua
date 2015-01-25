using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Framework;
using Linqua.Persistance;

namespace Linqua
{
    public class MainViewModel : ViewModelBase
    {
	    private WordListViewModel wordListViewModel;

	    public MainViewModel()
	    {
			if (DesignTimeDetection.IsInDesignTool)
			{
				WordListViewModel = new WordListViewModel(FakeData.FakeWords);
			}

			AddWordCommand = new DelegateCommand(AddWord);
	    }

	    public MainViewModel(ICompositionFactory compositionFactory, IEntryStorage storage)
			: this()
	    {
		    CompositionFactory = compositionFactory;

		    InitializeWordListAsync(compositionFactory, storage).FireAndForget();
	    }

		public ICommand AddWordCommand { get; private set; }

		[Import]
		public IEventLocator EventLocator { get; set; }

	    [Import]
        public IEventPublisher EventPublisher { get; set; }

		public IMainView View { get; set; }

        public WordListViewModel WordListViewModel
        {
	        get { return wordListViewModel; }
	        private set
	        {
		        wordListViewModel = value;
				RaisePropertyChanged();
	        }
        }

	    private async Task InitializeWordListAsync(ICompositionFactory compositionFactory, IEntryStorage storage)
		{
			var words = await storage.LoadAllEntries();

			WordListViewModel = compositionFactory.Create<WordListViewModel>(words);
		}

		private void AddWord()
		{
			View.NavigateToNewWordPage();
		}
    }
}