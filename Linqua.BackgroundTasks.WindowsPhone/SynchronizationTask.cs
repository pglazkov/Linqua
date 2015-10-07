using System;
using Windows.ApplicationModel.Background;
using Framework;
using Linqua.Logging;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
    // IMPORTANT: When renaming or moving the task don't forget to change the task declaration in the app manifest. 
    // Otherwise an exception will be thrown when registering the task ("Class not registered").
    public sealed class SynchronizationTask : IBackgroundTask
    {
		private static readonly ILogger Log;

		static SynchronizationTask()
		{
			LoggerHelper.SetupLogger();
			Log = LogManagerFactory.DefaultLogManager.GetLogger<SynchronizationTask>();
		}

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			if (Log.IsDebugEnabled)
				Log.Debug("Synchronization background task starting");

			var deferral = taskInstance.GetDeferral();

			try
			{
				var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();
				
				if (authenticatedSilently)
				{
					IBackendServiceClient storage = new MobileServiceBackendServiceClient(new SyncHandler(), new EventManager());
					await storage.InitializeAsync();
					await storage.TrySyncAsync();
				}
				else
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Authentication failed.");
				}

				if (Log.IsDebugEnabled)
					Log.Debug("Synchronization background task completed");
			}
			catch (Exception ex)
			{
				ExceptionHandlingHelper.HandleNonFatalError(ex, "Synchronization background task failed.", sendTelemetry: false);
			}
			finally
			{
				deferral.Complete();
			}
		}
    }
}
