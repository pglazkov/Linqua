using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Framework;

namespace Linqua
{
    public partial class RandomEntryListView : UserControl
    {
        public RandomEntryListView()
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
			
	    }

	    private void EntryLoaded(object sender, RoutedEventArgs e)
	    {
			var entryView = (Control)sender;

			var entryVm = (EntryListItemViewModel)entryView.DataContext;

		    if (entryVm == null)
		    {
			    return;
		    }

		    if (entryVm.JustAdded)
		    {
			    var itemView = EntryItemsControl.ContainerFromItem(entryVm) as Control;

			    if (itemView != null)
			    {
				    itemView.Focus(FocusState.Programmatic);
			    }

			    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			    {
				    EntryItemsControl.ScrollIntoView(entryVm);
			    });

			    entryVm.JustAdded = false;
		    }
	    }
    }
}
