using System;
using Framework.PlatformServices;

namespace Framework
{
	public class DispatcherProxy
	{
		private readonly IDispatcherService impl;

		private DispatcherProxy()
		{
			impl = PlatformService.Platform.CreateService<IDispatcherService>();
		}

		public static DispatcherProxy CreateDispatcher()
		{
			return new DispatcherProxy();
		}

		public bool CheckAccess()
		{
			return impl.CheckAccess();
		}

		public void BeginInvoke(Delegate method, params object[] args)
		{
			impl.BeginInvoke(method, args);
		}
	}
}