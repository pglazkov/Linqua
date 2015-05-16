using System;
using Framework;
using JetBrains.Annotations;

namespace Linqua.ViewModels.Behaviors
{
	public class RequestNextRandomEntryCommandBehavior : ViewModelBahviorBase<EntryListItemViewModel>
	{
		private bool canExecute;
		public const string Key = "RequestNextRandomEntryCommandBehavior";

	    public RequestNextRandomEntryCommandBehavior([NotNull] DelegateCommand requestNextRandomEntryCommand)
	    {
			Guard.NotNull(requestNextRandomEntryCommand, () => requestNextRandomEntryCommand);

			RequestNextRandomEntryCommand = requestNextRandomEntryCommand;
		    RequestNextRandomEntryCommand.CanExecuteChanged += OnCanExecuteChanged;

			UpdateCanExecute();
	    }

		public DelegateCommand RequestNextRandomEntryCommand { get; set; }

		public bool CanExecute
		{
			get { return canExecute; }
			private set
			{
				if (value == canExecute) return;
				canExecute = value;
				RaisePropertyChanged();
			}
		}

		private void OnCanExecuteChanged(object sender, EventArgs e)
		{
			UpdateCanExecute();
		}

		private void UpdateCanExecute()
		{
			CanExecute = RequestNextRandomEntryCommand.CanExecute();
		}
    }
}
