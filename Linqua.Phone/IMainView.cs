using Framework;
using Microsoft.Phone.Shell;

namespace Linqua
{
	public interface IMainView : INavigationView
	{
        IApplicationBarMenuItem AddWordButton { get; }
	}
}