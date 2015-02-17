using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Framework;

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

	    private void EntryLoaded(object sender, RoutedEventArgs e)
	    {
			var entryView = (Control)sender;

			var entryVm = (EntryListItemViewModel)entryView.DataContext;

			Guard.Assert(entryVm != null, "entryVm != null");

		    if (entryVm.JustAdded)
		    {
			    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			    {
				    entryView.Focus(FocusState.Programmatic);
			    });
		    }

		    //if (entryVm.JustAdded)
		    //{
		    //	var entryView = EntryItemsControl.ItemContainerGenerator.ContainerFromItem(entryVm);

		    //	var c = entryView as Control;


		    //	if (c != null)
		    //	{
		    //		c.Focus(FocusState.Programmatic);
		    //	}
		    //}
	    }
    }
}
