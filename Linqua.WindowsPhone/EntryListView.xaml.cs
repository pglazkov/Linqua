using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Linqua
{
    public partial class EntryListView : UserControl
    {
        public EntryListView()
        {
            InitializeComponent();
        }

	    private void EntryHolding(object sender, HoldingRoutedEventArgs e)
	    {
			var senderElement = sender as FrameworkElement;
			var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
			
			flyoutBase.ShowAt(senderElement);
	    }

	    private void EntryPressed(object sender, PointerRoutedEventArgs e)
	    {
			//var senderElement = sender as FrameworkElement;
			//var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

			//flyoutBase.ShowAt(senderElement);
	    }
    }
}
