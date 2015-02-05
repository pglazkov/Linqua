using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Framework;
using Framework.PlatformServices;
using Linqua.Events;
using Linqua.Persistance;
using System;
using Linqua.DataObjects;

namespace Linqua
{
    public class MainViewModel : ViewModelBase
    {
	    private readonly IEntryStorage storage;
	    private readonly IEventAggregator eventAggregator;
	    private readonly IStatusBusyService statusBusyService;
	    private EntryListViewModel entryListViewModel;
	    private bool isLoadingEntries;

	    public MainViewModel()
	    {
			if (DesignTimeDetection.IsInDesignTool)
			{
				EntryListViewModel = new EntryListViewModel(FakeData.FakeWords);
				EntryCreationViewModel = new EntryCreationViewModel(DesignTimeHelper.EventAggregator);
			}

			AddWordCommand = new DelegateCommand(AddWord);
	    }

	    public MainViewModel(ICompositionFactory compositionFactory, IEntryStorage storage, IEventAggregator eventAggregator, IStatusBusyService statusBusyService)
			: this()
	    {
		    Guard.NotNull(compositionFactory, () => compositionFactory);
		    Guard.NotNull(storage, () => storage);
		    Guard.NotNull(eventAggregator, () => eventAggregator);
		    Guard.NotNull(statusBusyService, () => statusBusyService);

		    this.storage = storage;
		    this.eventAggregator = eventAggregator;
		    this.statusBusyService = statusBusyService;

		    CompositionFactory = compositionFactory;

		    EntryCreationViewModel = compositionFactory.Create<EntryCreationViewModel>();

		    eventAggregator.GetEvent<EntryCreationRequestedEvent>().Subscribe(OnEntryCreationRequested);
		    eventAggregator.GetEvent<EntryDeletionRequestedEvent>().Subscribe(OnEntryDeletionRequested);
	    }

	    public ICommand AddWordCommand { get; private set; }		

		public IMainView View { get; set; }

		public EntryCreationViewModel EntryCreationViewModel { get; private set; }

        public EntryListViewModel EntryListViewModel
        {
	        get { return entryListViewModel; }
	        private set
	        {
		        entryListViewModel = value;
				RaisePropertyChanged();
	        }
        }

	    public bool IsLoadingEntries
	    {
		    get { return isLoadingEntries; }
		    private set
		    {
			    if (value.Equals(isLoadingEntries)) return;
			    isLoadingEntries = value;
			    RaisePropertyChanged();
		    }
	    }

	    public void Initialize()
	    {
			InitializeAsync().FireAndForget();
	    }

		private async Task InitializeAsync()
		{
			await storage.InitializeAsync();
			await InitializeWordListAsync(CompositionFactory, storage);
		}

	    private async Task InitializeWordListAsync(ICompositionFactory compositionFactory, IEntryStorage storage)
	    {
		    using (statusBusyService.Busy())
		    {
			    IsLoadingEntries = true;

			    try
			    {
				    var words = await storage.LoadAllEntries();

				    EntryListViewModel = compositionFactory.Create<EntryListViewModel>(words);
			    }
			    finally
			    {
				    IsLoadingEntries = false;
			    }
		    }
		}

		private void AddWord()
		{
			View.NavigateToNewWordPage();
		}

		private async void OnEntryCreationRequested(EntryCreationRequestedEvent e)
		{
			var newEntry = new ClientEntry(e.EntryText);

			using (statusBusyService.Busy("Saving..."))
			{
				var addedEntry = await storage.AddEntry(newEntry);

				EntryListViewModel.AddEntry(addedEntry);

				EntryCreationViewModel.Clear();

				EventAggregator.Publish(new EntryCreatedEvent(addedEntry));
			}
		}

	    private async void OnEntryDeletionRequested(EntryDeletionRequestedEvent e)
	    {
		    using (statusBusyService.Busy("Deleting..."))
		    {
			    await storage.DeleteEntry(e.EntryToDelete);

			    EntryListViewModel.DeleteEntryFromUI(e.EntryToDelete);
		    }
	    }

    }
}