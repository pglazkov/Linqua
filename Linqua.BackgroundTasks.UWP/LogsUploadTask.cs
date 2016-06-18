using System;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Framework;
using Framework.PlatformServices;
using Linqua.Logging;
using Linqua.Persistence;
using Linqua.Persistence.Exceptions;
using MetroLog;

namespace Linqua
{
    // IMPORTANT: When renaming or moving the task don't forget to change the task declaration in the app manifest. 
    // Otherwise an exception will be thrown when registering the task ("Class not registered").
    public sealed class LogsUploadTask : IBackgroundTask
    {
        private static readonly ILoggerAsync Log;

        static LogsUploadTask()
        {
            Bootstrapper.Run(typeof(LogsUploadTask));

            Log = (ILoggerAsync)LogManagerFactory.DefaultLogManager.GetLogger<LogsUploadTask>();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                await Log.InfoAsync("LogsUpload background task started");

                var logsUploadPending = Equals(ApplicationData.Current.LocalSettings.Values[LocalSettingsKeys.LogsUploadPending], true);

                if (!logsUploadPending)
                {
                    await Log.InfoAsync("There are no pending logs to upload.");
                    return;
                }

                var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

                if (authenticatedSilently)
                {
                    IBackendServiceClient storage = new MobileServiceBackendServiceClient(new SyncHandler(), new EventManager(), new LocalSettingsService());
                    await storage.InitializeAsync();

                    var logsUploadService = new LogSharingService(storage);

                    var uri = await logsUploadService.ShareCurrentLogAsync();

                    await Log.DebugAsync("Log is shared at: " + uri);

                    ApplicationData.Current.LocalSettings.Values[LocalSettingsKeys.LogsUploadPending] = false;
                }
                else
                {
                    await Log.WarnAsync("Authentication failed.");
                }

                await Log.InfoAsync("LogsUpload background task completed");
            }
            catch (NoInternetConnectionException)
            {
                await Log.InfoAsync("There is no internet connection. Do nothing.");
            }
            catch (Exception ex)
            {
                await ExceptionHandlingHelper.HandleNonFatalErrorAsync(ex, "LogsUpload background task failed.", sendTelemetry: false);
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}