using Framework;
using Linqua.Events;

namespace Linqua.UI
{
	public class EntryCreationViewModel : ViewModelBase
	{
		private readonly IEventAggregator eventAggregator;
		private string entryText;
		private bool isCreatingEntry;

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

		public bool IsCreatingEntry
		{
			get { return isCreatingEntry; }
			set
			{
				if (value.Equals(isCreatingEntry)) return;
				isCreatingEntry = value;
				RaisePropertyChanged();
				AddCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanAdd()
		{
			return !string.IsNullOrEmpty(EntryText) && !IsCreatingEntry;
		}

		private void Add()
		{
			IsCreatingEntry = true;

			eventAggregator.Publish(new EntryCreationRequestedEvent(EntryText));
		}

		public void Clear()
		{
			IsCreatingEntry = false;

			EntryText = string.Empty;
		}
	}
}