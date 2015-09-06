using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Framework.PlatformServices
{
	public static class DispatcherServiceExtensions
	{
		public static Task InvokeAsync([NotNull] this IDispatcherService service, [NotNull] Action action)
		{
			Guard.NotNull(service, nameof(service));
			Guard.NotNull(action, nameof(action));

			return service.InvokeAsync(action);
		}
	}
}