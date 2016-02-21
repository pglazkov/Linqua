using System;
using System.Reactive.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Framework;

namespace Linqua.UI
{
	public partial class EntryTextEditorView : UserControl
	{
		public EntryTextEditorView()
		{
			InitializeComponent();
		}

		#region InputTargetLostFocus Event

		public event EventHandler InputTargetLostFocus;

		protected virtual void OnInputTargetLostFocus()
		{
			var handler = InputTargetLostFocus;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		#endregion

		private EntryTextEditorViewModel ViewModel => (EntryTextEditorViewModel)DataContext;

	    public void FocusInputTarget()
		{
			NewEntryTextBox.Focus(FocusState.Programmatic);
		}

		private void EntryTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter && !ViewModel.IsBusy)
			{
				if (ViewModel.FinishCommand.CanExecute())
				{
					ViewModel.FinishCommand.Execute().FireAndForget();
				}
			}
		}

		private void EntryTextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			OnInputTargetLostFocus();
		}
	}
}
