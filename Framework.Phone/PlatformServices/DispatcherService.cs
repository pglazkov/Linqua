using System;
using System.Composition;
using System.Windows;
using System.Windows.Threading;

namespace Framework.PlatformServices
{
	/// <summary>
	/// Hides the dispatcher mis-match between Silverlight and .Net, largely so code reads a bit easier
	/// </summary>
	[Export(typeof(IDispatcherService))]
	public class DispatcherService : IDispatcherService
	{
		readonly Dispatcher innerDispatcher;

		private DispatcherService(Dispatcher dispatcher)
		{
			innerDispatcher = dispatcher;
		}

		public static DispatcherService CreateDispatcher()
		{
			DispatcherService result;
#if SILVERLIGHT
                if (Deployment.Current == null)
                    return null;

                result = new DispatcherService(Deployment.Current.Dispatcher);
#else
			if (Application.Current == null)
				return null;

			result = new DispatcherService(Application.Current.DispatcherService);
#endif
				return result;

		}

		public bool CheckAccess()
		{
			return innerDispatcher.CheckAccess();
		}

		public void BeginInvoke(Delegate method, params Object[] args)
		{
#if SILVERLIGHT
			innerDispatcher.BeginInvoke(method, args);
#else
			return innerDispatcher.BeginInvoke(method, DispatcherPriority.Normal, args);
#endif
		}
	}
}