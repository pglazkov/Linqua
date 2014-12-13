using Windows.UI.Xaml.Controls;

namespace Linqua
{
	public interface IMainView : INavigationView
	{
		Button AddWordButton { get; }
	}
}