using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua
{
	public sealed partial class RandomEntryListItemView : UserControl
	{
		public RandomEntryListItemView()
		{
			this.InitializeComponent();

			DataContextChanged += OnDataContextChanged;
		}

		private EntryListItemViewModel ViewModel
		{
			get { return (EntryListItemViewModel)DataContext; }
		}

		private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			var storyboard = (Storyboard)Resources["DataContextChangedStoryboard"];
			storyboard.Begin();
		}
	}
}
