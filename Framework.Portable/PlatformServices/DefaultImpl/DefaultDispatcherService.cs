using System;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
    [DefaultExport(typeof(IDispatcherService))]
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