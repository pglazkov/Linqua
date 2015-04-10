using System;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Persistence;
using Linqua.Translation;

namespace Linqua
{
	public class EntryDetailsViewModel : EntryViewModel
	{
		private readonly IDataStore storage;
		private readonly IStatusBusyService statusBusyService;
		private Lazy<ITranslationService> translator;
		private bool isLoadingData;

		public EntryDetailsViewModel()
		{
			GoHomeCommand = new DelegateCommand(GoHome);
		}

		public EntryDetailsViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			Lazy<ITranslationService> translator)
			: this()
		{
			Guard.NotNull(compositionFactory, () => compositionFactory);
			Guard.NotNull(storage, () => storage);
			Guard.NotNull(eventAggregator, () => eventAggregator);
			Guard.NotNull(statusBusyService, () => statusBusyService);
			Guard.NotNull(translator, () => translator);

			this.storage = storage;
			this.statusBusyService = statusBusyService;
			this.translator = translator;

			EventAggregator = eventAggregator;
			CompositionFactory = compositionFactory;
		}

		public DelegateCommand GoHomeCommand { get; private set; }

		public bool IsLoadingData
		{
			get { return isLoadingData; }
			private set
			{
				if (value == isLoadingData) return;
				isLoadingData = value;
				RaisePropertyChanged();
			}
		}

		public string EntryText
		{
			get { return Entry != null ? Entry.Text : string.Empty; }
		}

		public IEntryDetailsView View { get; set; }

		public async Task InitializeAsync([NotNull] string entryId)
		{
			Guard.NotNullOrEmpty(entryId, () => entryId);

			using (statusBusyService.Busy())
			{
				IsLoadingData = true;

				try
				{
					await storage.InitializeAsync();

					Entry = await storage.LookupById(entryId);
				}
				finally
				{
					IsLoadingData = false;
				}
			}
		}

		private void GoHome()
		{
			View.NavigateHome();
		}

		protected override void OnEntryChangedOverride()
		{
			RaisePropertyChanged(() => EntryText);
		}
	}
}
