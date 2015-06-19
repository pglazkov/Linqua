using System;

namespace Linqua.Framework
{
	public interface INavigationView
	{
		bool Navigate(Type destination);
		bool Navigate(Type destination, object parameter);
	}
}