using Framework;
using Linqua.Events;

namespace Linqua
{
	public class EntryCreationViewModel : ViewModelBase
	{
		private readonly IEventAggregator eventAggregator;
		private string entryText;

		public EntryCreationViewModel()
			: this(DesignTimeDetection.IsInDesignTool ? DesignTimeHelper.EventAggregator : null)
		{
		}

		public EntryCreationViewModel(IEventAggregator eventAggregator)
		{
			Guard.NotNull(eventAggregator, () => eventAggregator);

			this.eventAggregator = eventAggregator;
			AddCommand = new DelegateCommand(Add, CanAdd);
		}

		public DelegateCommand AddCommand { get; private set; }

		public string EntryText
		{
			get { return entryText; }
			set
			{
				if (value == entryText) return;
				entryText = value;
				RaisePropertyChanged();
				AddCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanAdd()
		{
			return !string.IsNullOrEmpty(EntryText);
		}

		private void Add()
		{
			eventAggregator.Publish(new EntryCreationRequestedEvent(EntryText));
		}

		public void Clear()
		{
			EntryText = string.Empty;
		}
	}
}