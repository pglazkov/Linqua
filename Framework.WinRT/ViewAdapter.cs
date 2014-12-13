using System;
using Windows.UI.Xaml;

namespace Framework
{
	public class ViewAdapter : IView
	{
		private FrameworkElement Element { get; set; }

		public ViewAdapter(FrameworkElement element)
		{
			Guard.NotNull(() => element);

			Element = element;

			Element.Loaded += (o, e) => OnLoaded();
			Element.Unloaded += (o, e) => OnUnloaded();
		}

		#region Loaded Event

		public event EventHandler<EventArgs> Loaded;

		protected virtual void OnLoaded()
		{
			EventHandler<EventArgs> handler = Loaded;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		#endregion

		#region Unloaded Event

		public event EventHandler<EventArgs> Unloaded;

		protected virtual void OnUnloaded()
		{
			EventHandler<EventArgs> handler = Unloaded;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		#endregion

		public object DataContext
		{
			get { return Element.DataContext; }
			set { Element.DataContext = value; }
		}

		public T FindChildName<T>(string controlName) where T : class
		{
			return Element.FindName(controlName) as T;
		}
	}
}