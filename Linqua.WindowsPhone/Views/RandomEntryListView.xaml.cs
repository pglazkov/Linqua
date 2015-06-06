using System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Framework;
using Linqua.Events;

namespace Linqua
{
    public partial class RandomEntryListView : UserControl
    {
        public RandomEntryListView()
        {
            InitializeComponent();
        }

	    private EntryListViewModel ViewModel
	    {
		    get { return (EntryListViewModel)DataContext; }
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
			//var entryView = (Control)sender;

			//var entryVm = (EntryListItemViewModel)entryView.DataContext;

			//if (entryVm == null)
			//{
			//	return;
			//}

			//if (entryVm.JustAdded)
			//{
			//	var itemView = EntryItemsControl.ContainerFromItem(entryVm) as Control;

			//	if (itemView != null)
			//	{
			//		itemView.Focus(FocusState.Programmatic);
			//	}

			//	Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			//	{
			//		EntryItemsControl.ScrollIntoView(entryVm);
			//	});

			//	entryVm.JustAdded = false;
			//}
	    }

		private void OnItemClicked(object sender, ItemClickEventArgs e)
		{
			var entryVm = (EntryListItemViewModel)e.ClickedItem;

			var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();
			
			eventAggregator.Publish(new EntryDetailsRequestedEvent(entryVm.Entry.Id));
		}

	    private void OnItemFlickedAway(object sender, EventArgs e)
	    {
		    if (ViewModel.ShowNextEntriesCommand.CanExecute())
		    {
			    ViewModel.ShowNextEntriesCommand.Execute().FireAndForget();
		    }
	    }
    }
}
