using Framework;
using Microsoft.Phone.Shell;

namespace Linqua
{
	public interface IMainView : INavigationView
	{
		ApplicationBarIconButton AddWordButton { get; }
	}
}