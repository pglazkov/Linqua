using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Framework;

namespace Linqua
{
	public partial class EntryListItemView : UserControl, IEntryListItemView
	{
		private bool isLoaded;

		public EntryListItemView()
		{
			InitializeComponent();

			DataContextChanged += OnDataContextChanged;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		private EntryListItemViewModel ViewModel
		{
			get { return (EntryListItemViewModel)DataContext; }
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			isLoaded = true;

			AnimateJustAddedIfNeeded();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			isLoaded = false;
		}

		private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			var newVm = (EntryListItemViewModel)args.NewValue;

			if (newVm != null)
			{
				newVm.View = this;
			}

			if (isLoaded)
			{
				AnimateJustAddedIfNeeded();
			}
		}

		private void AnimateJustAddedIfNeeded()
		{
			if (ViewModel == null)
			{
				return;
			}

			if (ViewModel.JustAdded)
			{
				var storyboard = (Storyboard)Resources["FlashBackgroundStoryboard"];

				storyboard.Begin();
			}
		}

		public void Focus()
		{
			var ownerItemsControl = this.GetFirstAncestorOfType<ItemsControl>();

			if (ownerItemsControl != null && ViewModel != null)
			{
				var listView = ownerItemsControl as ListView;

				Control listItemContainer = null;

				if (listView != null)
				{
					listItemContainer = listView.ContainerFromItem(ViewModel) as Control;
				}
				else if (ownerItemsControl.ItemContainerGenerator != null)
				{
					listItemContainer = ownerItemsControl.ItemContainerGenerator.ContainerFromItem(ViewModel) as Control;
				}

				if (listItemContainer != null)
				{
					listItemContainer.Focus(FocusState.Programmatic);
				}
			}
		}
	}
}
