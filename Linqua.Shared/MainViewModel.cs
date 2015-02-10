﻿using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Framework;
using Framework.PlatformServices;
using Linqua.Events;
using Linqua.Persistence;
using System;
using System.Linq;
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

			EntryListViewModel = compositionFactory.Create<EntryListViewModel>();
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
			await InitializeWordListAsync(CompositionFactory, storage);
		}

	    private async Task InitializeWordListAsync(ICompositionFactory compositionFactory, IEntryStorage storage)
	    {
			IsLoadingEntries = true;

		    using (statusBusyService.Busy())
		    {
			    try
			    {
					await storage.InitializeAsync();
				    var words = await storage.LoadAllEntries();

				    EntryListViewModel.Entries = words;
			    }
			    finally
			    {
				    if (EntryListViewModel.Entries.Any())
				    {
					    IsLoadingEntries = false;
				    }
			    }
		    }

			using (statusBusyService.Busy("Syncing..."))
		    {
			    try
			    {
					// Now when the data from cache is loaded and shown to the user sync with 
				    // the cloud and refresh the data.
				    await storage.EnqueueSync();
				    await RefreshAsync();
			    }
			    finally
			    {
				    IsLoadingEntries = false;
					EntryListViewModel.IsInitializationComplete = true;
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

	    public async Task RefreshAsync()
	    {
			var words = await storage.LoadAllEntries();

			EntryListViewModel.Entries = words;
	    }
    }
}