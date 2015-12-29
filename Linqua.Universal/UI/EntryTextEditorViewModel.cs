using Framework;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua.UI
{
	public class EntryTextEditorViewModel : ViewModelBase
	{
		private readonly IEventAggregator eventAggregator;
		private string entryText;
		private bool isBusy;
		private ClientEntry data;

		public EntryTextEditorViewModel()
			: this(DesignTimeDetection.IsInDesignTool ? DesignTimeHelper.EventAggregator : null)
		{
		}

		public EntryTextEditorViewModel(IEventAggregator eventAggregator)
		{
			Guard.NotNull(eventAggregator, nameof(eventAggregator));

			this.eventAggregator = eventAggregator;
			FinishCommand = new DelegateCommand(Finish, CanFinish);
		}

		public DelegateCommand FinishCommand { get; }

		public ClientEntry Data
		{
			get { return data; }
			set
			{
				if (Equals(value, data)) return;
				data = value;

				if (Data != null)
				{
					EntryText = Data.Text;
				}
				else
				{
					EntryText = null;
				}

				IsBusy = false;

				RaisePropertyChanged();
			}
		}

		public string EntryText
		{
			get { return entryText; }
			set
			{
				if (value == entryText) return;
				entryText = value;
				RaisePropertyChanged();
				FinishCommand.RaiseCanExecuteChanged();
			}
		}

		public bool IsBusy
		{
			get { return isBusy; }
			set
			{
				if (value.Equals(isBusy)) return;
				isBusy = value;
				RaisePropertyChanged();
				FinishCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanFinish()
		{
			return !string.IsNullOrEmpty(EntryText) && !IsBusy;
		}

		private void Finish()
		{
			IsBusy = true;

			if (data == null)
			{
				data = ClientEntry.CreateNew(EntryText);
			}
			else
			{
				Data.Text = EntryText;
			}

			eventAggregator.Publish(new EntryEditingFinishedEvent(Data));
		}

		public void Clear()
		{
			Data = null;
		}
	}
}