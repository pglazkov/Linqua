using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.Persistence;
using Linqua.Translation;

namespace Linqua.UI
{
    public class EntryEditViewModel : EntryViewModel
    {
        private readonly IBackendServiceClient storage;
        private readonly IStatusBusyService statusBusyService;
        private readonly IEntryOperations entryOperations;
        private Lazy<ITranslationService> translator;
        private bool isLoadingData;
        private CancellationTokenSource loadDataCts = new CancellationTokenSource();
        private bool isSaving;

        public EntryEditViewModel(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            TranslateCommand = new DelegateCommand(() => TranslateAsync().FireAndForget(), CanTranslate);
            SaveCommand = new DelegateCommand(() => SaveAsync().FireAndForget(), CanSave);
            CancelCommand = new DelegateCommand(Cancel, CanCancel);
        }

        [ImportingConstructor]
        public EntryEditViewModel(
            ICompositionFactory compositionFactory,
            IBackendServiceClient storage,
            IEventAggregator eventAggregator,
            IStatusBusyService statusBusyService,
            IEntryOperations entryOperations,
            Lazy<ITranslationService> translator)
            : this(eventAggregator)
        {
            Guard.NotNull(compositionFactory, nameof(compositionFactory));
            Guard.NotNull(storage, nameof(storage));
            Guard.NotNull(eventAggregator, nameof(eventAggregator));
            Guard.NotNull(statusBusyService, nameof(statusBusyService));
            Guard.NotNull(translator, nameof(translator));
            Guard.NotNull(entryOperations, nameof(entryOperations));

            this.storage = storage;
            this.statusBusyService = statusBusyService;
            this.entryOperations = entryOperations;
            this.translator = translator;

            EventAggregator = eventAggregator;
            CompositionFactory = compositionFactory;
        }

        public DelegateCommand TranslateCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public IEntryEditView View { get; set; }

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

        private bool IsSaving
        {
            get { return isSaving; }
            set
            {
                isSaving = value;
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

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

        protected override void OnIsDeletingSelfChanged()
        {
            base.OnIsDeletingSelfChanged();

            CancelCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        protected override void SetIsLearnt(bool value)
        {
            Telemetry.Client.TrackUserAction("ChangeIsLearned", TelemetryConstants.Features.WordEditor);

            Entry.IsLearnt = value;

            RaisePropertyChanged(nameof(IsLearnt));
            RaisePropertyChanged(nameof(IsLearnStatusText));
            RaisePropertyChanged(nameof(ToggleLearnedButtonHint));
        }

        private async Task SaveAsync()
        {
            Telemetry.Client.TrackUserAction("Save", TelemetryConstants.Features.WordEditor);

            IsSaving = true;

            try
            {
                using (statusBusyService.Busy(CommonBusyType.Saving))
                {
                    await entryOperations.UpdateEntryAsync(Entry);

                    View.NavigateBack();
                }
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool CanSave()
        {
            return Entry != null && !IsSaving && !IsDeletingSelf;
        }

        private bool CanCancel()
        {
            return !IsDeletingSelf;
        }

        private void Cancel()
        {
            View.NavigateBack();
        }

        private async Task TranslateAsync()
        {
            Telemetry.Client.TrackUserAction("Translate", TelemetryConstants.Features.WordEditor);

            await entryOperations.TranslateEntryItemAsync(Entry, new[] {this});
        }

        private bool CanTranslate()
        {
            return !string.IsNullOrEmpty(Text) && !IsTranslating;
        }

        protected override void OnTextChangedOverride()
        {
            base.OnTextChangedOverride();

            TranslateCommand.RaiseCanExecuteChanged();
        }

        protected override void OnIsTranslatingChangedOverride()
        {
            base.OnIsTranslatingChangedOverride();

            TranslateCommand.RaiseCanExecuteChanged();
        }

        protected override void OnDeleted()
        {
            View.NavigateBack();
        }

        protected override void OnEntryChangedOverride()
        {
            base.OnEntryChangedOverride();

            TranslateCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        protected override void CleanupOverride()
        {
            loadDataCts.Cancel();
            loadDataCts = new CancellationTokenSource();
        }
    }
}