using System;
using System.Threading;
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
		private readonly IBackendServiceClient storage;
		private readonly IStatusBusyService statusBusyService;
		private Lazy<ITranslationService> translator;
		private bool isLoadingData;
		private CancellationTokenSource loadDataCts = new CancellationTokenSource();

		public EntryDetailsViewModel(IEventAggregator eventAggregator)
			: base(eventAggregator)
		{
			GoHomeCommand = new DelegateCommand(GoHome);
		}

		public EntryDetailsViewModel(
			ICompositionFactory compositionFactory,
			IBackendServiceClient storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			Lazy<ITranslationService> translator)
			: this(eventAggregator)
		{
			Guard.NotNull(compositionFactory, nameof(compositionFactory));
			Guard.NotNull(storage, nameof(storage));
			Guard.NotNull(eventAggregator, nameof(eventAggregator));
			Guard.NotNull(statusBusyService, nameof(statusBusyService));
			Guard.NotNull(translator, nameof(translator));

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

		public IEntryDetailsView View { get; set; }

		public async Task InitializeAsync([NotNull] string entryId)
		{
			Guard.NotNullOrEmpty(entryId, nameof(entryId));

			using (statusBusyService.Busy())
			{
				IsLoadingData = true;

				try
				{
					var ct = loadDataCts.Token;

					await storage.InitializeAsync();

					Entry = await storage.LookupById(entryId, ct);
				}
				catch (OperationCanceledException)
				{
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
			RaisePropertyChanged(nameof(Text));
		}

		protected override void CleanupOverride()
		{
			loadDataCts.Cancel();
			loadDataCts = new CancellationTokenSource();
		}

	}
}
