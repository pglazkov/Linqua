using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua.UI
{
	public sealed partial class RandomEntryListItemView : UserControl
	{
		public RandomEntryListItemView()
		{
			this.InitializeComponent();

			DataContextChanged += OnDataContextChanged;
		}

		private EntryListItemViewModel ViewModel => (EntryListItemViewModel)DataContext;

	    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			var storyboard = (Storyboard)Resources["DataContextChangedStoryboard"];
			storyboard.Begin();
		}

		private void OnContainerHolding(object sender, HoldingRoutedEventArgs e)
		{
			var flyoutBase = FlyoutBase.GetAttachedFlyout(this);

			flyoutBase.ShowAt(RootBorder);
		}
	}
}
