using System;
using Framework;

namespace Linqua
{
	public interface INavigationView : IView
	{
		bool Navigate(Type destination);
		bool Navigate(Type destination, object parameter);
	}
}