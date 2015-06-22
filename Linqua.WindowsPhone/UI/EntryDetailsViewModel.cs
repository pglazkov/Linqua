using System;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.Persistence;
using Linqua.Translation;

namespace Linqua.UI
{
	public class EntryDetailsViewModel : EntryViewModel
	{
		private readonly IDataStore storage;
		private readonly IStatusBusyService statusBusyService;
		private Lazy<ITranslationService> translator;
		private bool isLoadingData;

		public EntryDetailsViewModel(IEventAggregator eventAggregator)
			: base(eventAggregator)
		{
			GoHomeCommand = new DelegateCommand(GoHome);
			MarkLearnedCommand = new DelegateCommand(() => IsLearnt = true);
			MarkNotLearnedCommand = new DelegateCommand(() => IsLearnt = false);
		}

		public EntryDetailsViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			Lazy<ITranslationService> translator)
			: this(eventAggregator)
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
		public DelegateCommand MarkLearnedCommand { get; private set; }
		public DelegateCommand MarkNotLearnedCommand { get; private set; }

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

		protected override void OnDeleted()
		{
			GoHome();
		}

		protected override void OnEntryChangedOverride()
		{
			RaisePropertyChanged(() => EntryText);
		}
	}
}
