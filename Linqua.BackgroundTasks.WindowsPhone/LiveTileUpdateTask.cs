using System;
using Windows.ApplicationModel.Background;
using Framework;
using Linqua.Notifications;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
    // IMPORTANT: When renaming or moving the task don't forget to change the task declaration in the app manifest. 
    // Otherwise an exception will be thrown when registering the task ("Class not registered").
    public sealed class LiveTileUpdateTask : IBackgroundTask
	{
		private static readonly ILogger Log;
		  
		static LiveTileUpdateTask()
		{
			Bootstrapper.Run(typeof(LiveTileUpdateTask));

			Log = LogManagerFactory.DefaultLogManager.GetLogger<LiveTileUpdateTask>();
		}

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			Log.Info("Live tile update background task started.");

			var deferral = taskInstance.GetDeferral();

			try
			{
				var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

				if (authenticatedSilently)
				{
					IBackendServiceClient storage = new MobileServiceBackendServiceClient(new SyncHandler(), new EventManager());
					await storage.InitializeAsync(doInitialPoolIfNeeded: false);

					var liveTileManager = new LiveTileManager(storage);

					await liveTileManager.UpdateTileAsync();
				}
				else
				{
					Log.Warn("Authentication failed.");
				}

				Log.Info("Live tile update background task completed");
			}
			catch (Exception ex)
			{
				ExceptionHandlingHelper.HandleNonFatalError(ex, "Live tile background task failed.", sendTelemetry: false);
			}
			finally
			{
				deferral.Complete();
			}
		}
	}
}