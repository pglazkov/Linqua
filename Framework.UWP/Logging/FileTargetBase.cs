using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using MetroLog;
using MetroLog.Layouts;
using MetroLog.Targets;
using Nito.AsyncEx;

namespace Framework.Logging
{
    /// <summary>
    /// Base class for file targets.
    /// </summary>
    public abstract class FileTargetBase : AsyncTarget
    {
        /// <summary>
        /// Gets an object that defines the file naming parameters.
        /// </summary>
        public FileNamingParameters FileNamingParameters { get; }

        /// <summary>
        /// Gets or sets the number of days to retain log files for.
        /// </summary>
        public int RetainDays { get; set; }

        protected const string LogFolderName = "MetroLogs";

        /// <summary>
        /// Holds the next cleanup time.
        /// </summary>
        protected DateTime NextCleanupUtc { get; set; }

        private readonly AsyncLock _lock = new AsyncLock();

        protected FileTargetBase(Layout layout)
            : base(layout)
        {
            FileNamingParameters = new FileNamingParameters();
            RetainDays = 30;
        }

        protected abstract Task EnsureInitialized();
        protected abstract Task DoCleanup(Regex pattern, DateTime threshold);
        public abstract Task<StorageFile> GetCompressedLogFile();

        internal async Task ForceCleanupAsync()
        {
            // threshold...
            var threshold = DateTime.UtcNow.AddDays(0 - RetainDays);

            // walk...
            var regex = FileNamingParameters.GetRegex();

            await DoCleanup(regex, threshold);
        }

        private async Task CheckCleanupAsync()
        {
            var now = DateTime.UtcNow;
            if (now < NextCleanupUtc || RetainDays < 1)
                return;

            try
            {
                // threshold...
                var threshold = now.AddDays(0 - RetainDays);

                // walk...
                var regex = FileNamingParameters.GetRegex();

                await DoCleanup(regex, threshold);
            }
            finally
            {
                // reset...
                NextCleanupUtc = DateTime.UtcNow.AddHours(1);
            }
        }

        sealed protected override async Task<LogWriteOperation> WriteAsyncCore(LogWriteContext context, LogEventInfo entry)
        {
            using (await _lock.LockAsync())
            {
                await EnsureInitialized();
                await CheckCleanupAsync();

                var filename = FileNamingParameters.GetFilename(context, entry);
                var contents = Layout.GetFormattedString(context, entry);

                return await DoWriteAsync(filename, contents, entry);
            }
        }

        protected abstract Task<LogWriteOperation> DoWriteAsync(string fileName, string contents, LogEventInfo entry);
    }
}