using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Linqua
{
	public partial class EntryListItemView : UserControl, IEntryListViewModelView
	{
		public EntryListItemView()
		{
			InitializeComponent();

			DataContextChanged += OnDataContextChanged;
			Loaded += OnLoaded;
		}

		private EntryListItemViewModel ViewModel
		{
			get { return (EntryListItemViewModel)DataContext; }
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (ViewModel.JustAdded)
			{
				var storyboard = (Storyboard)Resources["FlashBackgroundStoryboard"];

				storyboard.Begin();
			}
		}

		private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			var newVm = (EntryListItemViewModel)args.NewValue;

			if (newVm != null)
			{
				newVm.View = this;
			}
		}
	}
}
