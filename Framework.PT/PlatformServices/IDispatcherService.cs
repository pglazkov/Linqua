using System;

namespace Framework.PlatformServices
{
	public interface IDispatcherService
	{
		bool CheckAccess();

		void BeginInvoke(Delegate method, params Object[] args);
	}
}