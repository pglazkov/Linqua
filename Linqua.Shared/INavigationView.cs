using System;

namespace Linqua
{
	public interface INavigationView
	{
		bool Navigate(Type destination);
		bool Navigate(Type destination, object parameter);
	}
}