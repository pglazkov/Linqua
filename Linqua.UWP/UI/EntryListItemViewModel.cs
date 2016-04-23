using Framework;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua.UI
{
    public class EntryListItemViewModel : EntryViewModel
    {
        private bool isTranslationShown;

        public EntryListItemViewModel()
            : this(new EventManager())
        {
        }

        public EntryListItemViewModel(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            if (DesignTimeDetection.IsInDesignTool)
            {
                Entry = ClientEntry.CreateNew("Aankomst");
            }
        }

        public EntryListItemViewModel(ClientEntry entry, IEventAggregator eventAggregator, bool justAdded = false) : base(entry, eventAggregator)
        {
            JustAdded = justAdded;
        }

        public IEntryListItemView View { get; set; }

        public bool JustAdded { get; set; }

        public bool IsTranslationShown
        {
            get { return isTranslationShown; }
            set
            {
                if (value == isTranslationShown) return;
                isTranslationShown = value;
                RaisePropertyChanged();

                if (value)
                {
                    Telemetry.Client.TrackUserAction("ShowTranslation", TelemetryConstants.Features.RandomWords);
                }

                EventAggregator.Publish(new IsTranslationShownChangedEvent(this));
            }
        }

        public void ShowTranslation()
        {
            isTranslationShown = true;
            RaisePropertyChanged(nameof(IsTranslationShown));
        }

        public void Focus()
        {
            if (View != null)
            {
                View.Focus();
            }
        }
    }
}