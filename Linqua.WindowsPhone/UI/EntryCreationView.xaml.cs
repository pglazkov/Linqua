using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Framework;

namespace Linqua.UI
{
	public partial class EntryCreationView : UserControl
	{
		public EntryCreationView()
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

		private EntryCreationViewModel ViewModel
		{
			get { return (EntryCreationViewModel)DataContext; }
		}

		public void FocusInputTarget()
		{
			NewEntryTextBox.Focus(FocusState.Programmatic);
		}

		private void EntryTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter && !ViewModel.IsCreatingEntry)
			{
				if (ViewModel.AddCommand.CanExecute())
				{
					ViewModel.AddCommand.Execute().FireAndForget();
				}
			}
		}

		private void EntryTextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			OnInputTargetLostFocus();
		}
	}
}
