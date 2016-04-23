using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Framework;
using JetBrains.Annotations;
using Linqua.Logging;
using MetroLog;
using Nito.AsyncEx;

namespace Linqua.UI
{
    internal class LogSharingAction : IDisposable
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<LogSharingAction>();
        private Uri logUri;
        private readonly ILogSharingService logSharingService;
        private TaskCompletionSource tcs = new TaskCompletionSource();

        public LogSharingAction([NotNull] ILogSharingService logSharingService)
        {
            Guard.NotNull(logSharingService, nameof(logSharingService));

            this.logSharingService = logSharingService;
            var dtm = DataTransferManager.GetForCurrentView();

            dtm.DataRequested += OnLogFilesShareDataRequested;
        }

        public async Task ExecuteAsync()
        {
            tcs = new TaskCompletionSource();

            try
            {
                logUri = await logSharingService.ShareCurrentLogAsync();

                DataTransferManager.ShowShareUI();
            }
            catch (Exception ex)
            {
                ExceptionHandlingHelper.HandleNonFatalError(ex, "Unexpected exception occured while trying to share the log files.");
            }

            await tcs.Task;
        }

        private void OnLogFilesShareDataRequested(object sender, DataRequestedEventArgs args)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Prepearing compressed logs to share.");

            if (Log.IsDebugEnabled)
                Log.Debug("Deferral deadline is: {0}", args.Request.Deadline);

            Stopwatch sw = new Stopwatch();

            var deferral = args.Request.GetDeferral();

            sw.Start();

            try
            {
                args.Request.Data.Properties.Title = $"Linqua Logs - {DateTime.UtcNow:s} | {DeviceInfo.DeviceName}";
                args.Request.Data.Properties.Description = "Linqua compressed log files.";

                args.Request.Data.SetWebLink(logUri);
            }
            catch (Exception ex)
            {
                ExceptionHandlingHelper.HandleNonFatalError(ex, "Unexpected exception occured while trying to share the log files.");
            }
            finally
            {
                sw.Stop();

                if (Log.IsDebugEnabled)
                    Log.Debug("Compressed log file obtained at {0}", DateTime.Now);

                deferral.Complete();

                tcs.TrySetResult();
            }
        }

        #region Dispose Pattern

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                var dtm = DataTransferManager.GetForCurrentView();

                dtm.DataRequested -= OnLogFilesShareDataRequested;
            }
        }

        ~LogSharingAction()
        {
            Dispose(false);
        }

        #endregion
    }
}