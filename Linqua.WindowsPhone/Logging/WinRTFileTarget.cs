using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Framework;
using MetroLog;
using MetroLog.Layouts;
using MetroLog.Targets;
using FileTargetBase = Framework.Logging.FileTargetBase;

namespace Linqua.Logging
{
	public abstract class WinRTFileTarget : FileTargetBase
	{
		private static StorageFolder logFolder = null;

		protected WinRTFileTarget(Layout layout)
			: base(layout)
		{
		}

		public static async Task<StorageFolder> EnsureInitializedAsync()
		{
			if (logFolder == null)
			{
				var root = ApplicationData.Current.LocalFolder;

				logFolder = await root.CreateFolderAsync(LogFolderName, CreationCollisionOption.OpenIfExists);

			}
			return logFolder;
		}

		private async Task<Stream> GetCompressedLogs()
		{
			var ms = new MemoryStream();

			await ZipFile.CreateFromDirectory(logFolder, ms);
			ms.Position = 0;

			return ms;
		}

		public override async Task<IStorageFile> GetCompressedLogFile()
		{
			var stream = await GetCompressedLogs();

			if (stream != null)
			{
				// create a temp file
				var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
					string.Format("Log - {0}.zip", DateTime.UtcNow.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture)), CreationCollisionOption.ReplaceExisting);

				using (var ras = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
				{
					await stream.CopyToAsync(ras);
				}

				stream.Dispose();

				return file;
			}

			return null;
		}

		protected override Task EnsureInitialized()
		{
			return EnsureInitializedAsync();
		}

		sealed protected override async Task DoCleanup(Regex pattern, DateTime threshold)
		{

			var toDelete = new List<StorageFile>();
			foreach (var file in await logFolder.GetFilesAsync())
			{
				if (pattern.Match(file.Name).Success && file.DateCreated <= threshold)
					toDelete.Add(file);
			}

			//Queries are still not supported in Windows Phone 8.1. Ensure temp cleanup
#if WINDOWS_PHONE_APP
            var zipPattern = new Regex(@"^Log(.*).zip$");
            foreach (var file in await ApplicationData.Current.TemporaryFolder.GetFilesAsync())
            {
                if (zipPattern.Match(file.Name).Success)
                    toDelete.Add(file);
            }
#else
			var qo = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { ".zip" })
			{
				FolderDepth = FolderDepth.Shallow,
				UserSearchFilter = "System.FileName:~<\"Log -\""
			};

			var query = ApplicationData.Current.TemporaryFolder.CreateFileQueryWithOptions(qo);

			var oldLogs = await query.GetFilesAsync();
			toDelete.AddRange(oldLogs);
#endif
			// walk...
			foreach (var file in toDelete)
			{
				try
				{
					await file.DeleteAsync();
				}
				catch (Exception ex)
				{
					InternalLogger.Current.Warn(string.Format("Failed to delete '{0}'.", file.Path), ex);
				}
			}
		}

		protected sealed override async Task<LogWriteOperation> DoWriteAsync(string fileName, string contents, LogEventInfo entry)
		{
			// write...

			var file = await logFolder.CreateFileAsync(fileName, FileNamingParameters.CreationMode == FileCreationMode.AppendIfExisting ? CreationCollisionOption.OpenIfExists : CreationCollisionOption.ReplaceExisting);

			// Write contents
			await WriteTextToFileCore(file, contents);

			// return...
			return new LogWriteOperation(this, entry, true);
		}

		protected abstract Task WriteTextToFileCore(IStorageFile file, string contents);
	}
}
