using System;

namespace Linqua.Framework
{
    public interface INavigationView
    {
        void Navigate(Type destination);
        void Navigate(Type destination, object parameter);
    }
}