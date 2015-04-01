using System;
using System.Threading.Tasks;
using Windows.Storage;
using Framework.Logging;
using MetroLog.Layouts;
using MetroLog.Targets;

namespace Linqua.Logging
{
	/// <summary>
	/// Defines a target that will append messages to a single file.
	/// </summary>
	public class FileStreamingTarget : WinRTFileTarget
	{
		public static readonly FileStreamingTarget Instance = new FileStreamingTarget();

		private FileStreamingTarget()
			: this(new LoggingLayout())
		{
		}

		private FileStreamingTarget(Layout layout)
			: base(layout)
		{
			FileNamingParameters.IncludeLevel = false;
			FileNamingParameters.IncludeLogger = false;
			FileNamingParameters.IncludeSequence = false;
			FileNamingParameters.IncludeSession = false;
			FileNamingParameters.IncludeTimestamp = FileTimestampMode.Date;
			FileNamingParameters.CreationMode = FileCreationMode.AppendIfExisting;

			RetainDays = 1;
		}

		protected override Task WriteTextToFileCore(IStorageFile file, string contents)
		{
			return FileIO.AppendTextAsync(file, contents + Environment.NewLine).AsTask();
		}
	}
}
