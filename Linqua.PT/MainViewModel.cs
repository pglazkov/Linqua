using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Framework;
using Linqua.Events;
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
				WordListViewModel = new WordListViewModel(DesignTimeHelper.FakeWords);
			}

			AddWordCommand = new DelegateCommand(AddWord);
	    }

	    public MainViewModel(ICompositionFactory compositionFactory, IWordStorage storage)
			: this()
	    {
		    CompositionFactory = compositionFactory;

		    InitializeWordListAsync(compositionFactory, storage).FireAndForget();
	    }

		public ICommand AddWordCommand { get; private set; }

	    [Import]
        public IEventPublisher EventPublisher { get; set; }

        public WordListViewModel WordListViewModel
        {
	        get { return wordListViewModel; }
	        private set
	        {
		        wordListViewModel = value;
				RaisePropertyChanged();
	        }
        }

	    private async Task InitializeWordListAsync(ICompositionFactory compositionFactory, IWordStorage storage)
		{
			var words = await storage.LoadAllWords();

			WordListViewModel = compositionFactory.Create<WordListViewModel>(words);
		}

		private void AddWord()
		{
			EventPublisher.Publish(new AddWordRequestedEvent());
		}
    }
}