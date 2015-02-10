using System;
using Windows.ApplicationModel.Background;
using Linqua.Persistence;

namespace Linqua.BackgroundTasks
{
	public sealed class SynchronizationTask : IBackgroundTask
    {
		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			var deferral = taskInstance.GetDeferral();

			try
			{
				var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

				if (authenticatedSilently)
				{
					await OfflineSync.InitializeAsync(new MobileServiceSyncHandler());
					await OfflineSync.TrySyncAsync();
				}
			}
			finally
			{
				deferral.Complete();
			}
		}
    }
}
