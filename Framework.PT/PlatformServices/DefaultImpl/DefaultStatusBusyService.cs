using System;
using System.Reactive.Disposables;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
	[DefaultExport(typeof(IStatusBusyService))]
	public class DefaultStatusBusyService : IStatusBusyService
	{
		public IDisposable Busy(string statusText = null)
		{
			return Disposable.Create(() => {});
		}
	}
}