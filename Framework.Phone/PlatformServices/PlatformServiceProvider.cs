using System;

namespace Framework.PlatformServices
{
	public class PlatformServiceProvider : IPlatformServiceProvider
	{
		public T CreateService<T>()
		{
			var type = typeof (T);
			
			if (type == typeof (IDesignModeDetectionService))
			{
				return (T) (object) new DesignModeDetectionService();
			}

			if (type == typeof (IViewLocationService))
			{
				return (T) (object) new ViewLocationService();
			}

			if (type == typeof (IDispatcherService))
			{
				return (T) (object) DispatcherService.CreateDispatcher();
			}

			throw new NotImplementedException();
		}
	}
}