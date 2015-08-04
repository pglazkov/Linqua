using System;
using Windows.ApplicationModel.Background;
using Framework;
using Linqua.Logging;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
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
					IDataStore storage = new MobileServiceDataStore(new SyncHandler(), new EventManager());
					await storage.InitializeAsync();
					await storage.EnqueueSync();
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
				if (Log.IsErrorEnabled)
					Log.Error("Synchronization background task failed.", ex);
			}
			finally
			{
				deferral.Complete();
			}
		}
    }
}
