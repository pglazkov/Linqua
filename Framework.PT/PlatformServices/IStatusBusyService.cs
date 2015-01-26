using System;

namespace Framework.PlatformServices
{
	public interface IStatusBusyService
	{
		IDisposable Busy(string statusText = null);
	}
}