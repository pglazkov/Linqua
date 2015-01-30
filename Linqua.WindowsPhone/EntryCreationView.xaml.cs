using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Linqua
{
	public partial class EntryCreationView : UserControl
	{
		public EntryCreationView()
		{
			InitializeComponent();
		}

		private EntryCreationViewModel ViewModel
		{
			get { return (EntryCreationViewModel)DataContext; }
		}

		private void EntryTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter && !ViewModel.IsCreatingEntry)
			{
				if (ViewModel.AddCommand.CanExecute())
				{
					ViewModel.AddCommand.Execute();
				}
			}
		}
	}
}
