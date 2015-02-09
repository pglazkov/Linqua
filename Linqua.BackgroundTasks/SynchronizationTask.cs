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

			var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

			if (authenticatedSilently)
			{
				await OfflineSync.InitializeAsync(new MobileServiceSyncHandler());

				try
				{
					await OfflineSync.SyncAsync();
				}
				catch (Exception e)
				{
					// Synchronization failed, will try next time...

					// TODO: Log the error.
				}
			}

			deferral.Complete();
		}
    }
}
