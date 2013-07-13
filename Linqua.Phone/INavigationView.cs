using System.Windows.Navigation;
using Framework;

namespace Linqua
{
	public interface INavigationView : IView
	{
		NavigationService NavigationService { get; }
	}
}