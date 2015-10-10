using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nito.AsyncEx;

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

		public static Task InvokeAsync([NotNull] this IDispatcherService service, [NotNull] Func<Task> action)
		{
			Guard.NotNull(service, nameof(service));
			Guard.NotNull(action, nameof(action));

			var tcs = new TaskCompletionSource();

			service.InvokeAsync(new Action(() =>
			{
				action().ContinueWith(t =>
				{
					if (t.Exception != null)
					{
						tcs.TrySetException(t.Exception);
						return;
					}

					if (t.IsCanceled)
					{
						tcs.TrySetCanceled();
					}

					tcs.TrySetResult();
				});
			}));

			return tcs.Task;
		}
	}
}