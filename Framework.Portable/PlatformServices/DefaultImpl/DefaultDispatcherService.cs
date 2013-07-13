using System;

namespace Framework.PlatformServices.DefaultImpl
{
	public class DefaultDispatcherService : IDispatcherService
	{
		public bool CheckAccess()
		{
			return true;
		}

		public void BeginInvoke(Delegate method, params object[] args)
		{
			method.DynamicInvoke(args);
		}
	}
}