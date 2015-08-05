using System;
using System.Threading.Tasks;
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

		public Task InvokeAsync(Delegate method, params object[] args)
		{
			method.DynamicInvoke(args);

		    return Task.FromResult(true);
		}
	}
}