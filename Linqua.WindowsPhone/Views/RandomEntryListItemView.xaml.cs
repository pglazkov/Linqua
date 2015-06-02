using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua
{
	public sealed partial class RandomEntryListItemView : UserControl
	{
		public RandomEntryListItemView()
		{
			this.InitializeComponent();
		}

		private EntryListItemViewModel ViewModel
		{
			get { return (EntryListItemViewModel)DataContext; }
		}

		private void OnFlipBackButtonPressed(object sender, RoutedEventArgs e)
		{
			FlipControl.IsFlipped = true;
		}

		private void OnFlipFrontButtonPressed(object sender, RoutedEventArgs e)
		{
			FlipControl.IsFlipped = false;
		}

		private void SwipeDetectionBehavior_OnHorizontalSwipe(GestureRecognizer sender, CrossSlidingEventArgs args)
		{
			ViewModel.IsLearnt = true;
		}
	}
}
