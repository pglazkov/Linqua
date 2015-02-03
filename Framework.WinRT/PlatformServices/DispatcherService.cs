using System;
using System.Composition;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Framework.PlatformServices
{
	/// <summary>
	/// Hides the dispatcher mis-match between Silverlight and .Net, largely so code reads a bit easier
	/// </summary>
	[Export(typeof(IDispatcherService))]
	public class DispatcherService : IDispatcherService
	{
		readonly CoreDispatcher innerDispatcher;

		private DispatcherService(CoreDispatcher dispatcher)
		{
			innerDispatcher = dispatcher;
		}

		public static DispatcherService CreateDispatcher()
		{
			DispatcherService result;

			if (Application.Current == null)
				return null;

			result = new DispatcherService(CoreApplication.MainView.CoreWindow.Dispatcher);

			return result;

		}

		public bool CheckAccess()
		{
			return innerDispatcher.HasThreadAccess;
		}

		public void BeginInvoke(Delegate method, params Object[] args)
		{
			innerDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => method.DynamicInvoke(args)).AsTask().FireAndForget();
		}
	}
}