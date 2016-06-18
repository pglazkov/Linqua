using System;
using Windows.ApplicationModel.Background;
using Framework;
using Framework.PlatformServices;
using Linqua.Notifications;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
    // IMPORTANT: When renaming or moving the task don't forget to change the task declaration in the app manifest. 
    // Otherwise an exception will be thrown when registering the task ("Class not registered").
    public sealed class LiveTileUpdateTask : IBackgroundTask
    {
        private static readonly ILoggerAsync Log;

        static LiveTileUpdateTask()
        {
            Bootstrapper.Run(typeof(LiveTileUpdateTask));

            Log = (ILoggerAsync)LogManagerFactory.DefaultLogManager.GetLogger<LiveTileUpdateTask>();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                await Log.InfoAsync("Live tile update background task started.");

                var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

                if (authenticatedSilently)
                {
                    IBackendServiceClient storage = new MobileServiceBackendServiceClient(new SyncHandler(), new EventManager(), new LocalSettingsService());
                    await storage.InitializeAsync();

                    var liveTileManager = new LiveTileManager(storage);

                    await liveTileManager.UpdateTileAsync();
                }
                else
                {
                    await Log.WarnAsync("Authentication failed.");
                }

                await Log.InfoAsync("Live tile update background task completed");
            }
            catch (Exception ex)
            {
                await ExceptionHandlingHelper.HandleNonFatalErrorAsync(ex, "Live tile background task failed.", sendTelemetry: false);
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}