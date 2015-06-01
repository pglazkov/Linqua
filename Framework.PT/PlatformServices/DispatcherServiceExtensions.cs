using System;
using JetBrains.Annotations;

namespace Framework.PlatformServices
{
	public static class DispatcherServiceExtensions
	{
		public static void BeginInvoke([NotNull] this IDispatcherService service, [NotNull] Action action)
		{
			Guard.NotNull(service, () => service);
			Guard.NotNull(action, () => action);

			service.BeginInvoke(action);
		}
	}
}