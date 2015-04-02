using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Framework;
using Linqua.Events;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua
{
	public sealed partial class FullEntryListView : UserControl
	{
		public FullEntryListView()
		{
			this.InitializeComponent();
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

		private void OnItemClicked(object sender, ItemClickEventArgs e)
		{
			var entryVm = (EntryListItemViewModel)e.ClickedItem;

			var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();

			eventAggregator.Publish(new EntryDetailsRequestedEvent(entryVm.Entry.Id));
		}
	}
}
