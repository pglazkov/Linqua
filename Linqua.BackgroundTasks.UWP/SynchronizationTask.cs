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
        private static readonly ILoggerAsync Log;

        static SynchronizationTask()
        {
            Bootstrapper.Run(typeof(SynchronizationTask));

            Log = (ILoggerAsync)LogManagerFactory.DefaultLogManager.GetLogger<SynchronizationTask>();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                await Log.InfoAsync("Synchronization background task started");

                var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

                if (authenticatedSilently)
                {
                    IBackendServiceClient storage = new MobileServiceBackendServiceClient(new SyncHandler(), new EventManager());
                    await storage.InitializeAsync();
                    await storage.TrySyncAsync();
                }
                else
                {
                    await Log.WarnAsync("Authentication failed.");
                }

                await Log.InfoAsync("Synchronization background task completed");
            }
            catch (Exception ex)
            {
                await ExceptionHandlingHelper.HandleNonFatalErrorAsync(ex, "Synchronization background task failed.", sendTelemetry: false);
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}