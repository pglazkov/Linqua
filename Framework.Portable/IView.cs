using System;

namespace Framework
{
	public interface IView
	{
		event EventHandler<EventArgs> Loaded;
		event EventHandler<EventArgs> Unloaded;

		object DataContext { get; set; }

		T FindChildName<T>(string controlName) where T : class;
	}
}