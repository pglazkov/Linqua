using System;

namespace Framework.PlatformServices.DefaultImpl
{
	public class DefaultPlatformServiceProvider : IPlatformServiceProvider
	{
		public T CreateService<T>()
		{
			var type = typeof (T);
			
			if (type == typeof (IDesignModeDetectionService))
			{
				return (T)(object)new DefaultDesignModeDetectionService();
			}

			if (type == typeof (IViewLocationService))
			{
				return (T) (object) new DefaultViewLocationService();
			}

			if (type == typeof (IDispatcherService))
			{
				return (T) (object) new DefaultDispatcherService();
			}

			throw new NotImplementedException();
		}
	}
}