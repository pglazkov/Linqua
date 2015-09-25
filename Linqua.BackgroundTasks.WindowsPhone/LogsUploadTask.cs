using System;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Framework;
using Linqua.Logging;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
    // IMPORTANT: When renaming or moving the task don't forget to change the task declaration in the app manifest. 
    // Otherwise an exception will be thrown when registering the task ("Class not registered").
    public sealed class LogsUploadTask : IBackgroundTask
    {
		private static readonly ILogger Log;

		static LogsUploadTask()
		{
			LoggerHelper.SetupLogger();
			Log = LogManagerFactory.DefaultLogManager.GetLogger<LogsUploadTask>();
		}

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			if (Log.IsDebugEnabled)
				Log.Debug("LogsUpload background task starting");

			var logsUploadPending = Equals(ApplicationData.Current.LocalSettings.Values[LocalSettingsKeys.LogsUploadPending], true);

			if (!logsUploadPending)
			{
				Log.Debug("There are no pending logs to upload.");
				return;
			}

			var deferral = taskInstance.GetDeferral();

			try
			{
				var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();
				
				if (authenticatedSilently)
				{
					IBackendServiceClient storage = new MobileServiceBackendServiceClient(new SyncHandler(), new EventManager());
					var logsUploadService = new LogSharingService(storage);

					var uri = await logsUploadService.ShareCurrentLogAsync();

					Log.Debug("Log is shared at: " + uri);

					ApplicationData.Current.LocalSettings.Values[LocalSettingsKeys.LogsUploadPending] = false;
				}
				else
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Authentication failed.");
				}

				if (Log.IsDebugEnabled)
					Log.Debug("LogsUpload background task completed");
			}
			catch (Exception ex)
			{
				if (Log.IsErrorEnabled)
					Log.Error("LogsUpload background task failed.", ex);
			}
			finally
			{
				deferral.Complete();
			}
		}
    }
}
