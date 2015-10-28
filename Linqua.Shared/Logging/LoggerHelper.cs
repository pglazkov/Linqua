using Framework.Logging;
using MetroLog;
using MetroLog.Targets;

namespace Linqua.Logging
{
	internal static class LoggerHelper
	{
		private static readonly object SyncRoot = new object();
		private static bool isSetUp = false;

		public static void SetupLogger()
		{
			lock (SyncRoot)
			{
				if (isSetUp)
				{
					return;
				}

				var configuration = new LoggingConfiguration();
#if DEBUG
				configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget(new LoggingLayout()));
#endif
#if DEBUG
				var minLogLevelForFileTarget = LogLevel.Debug;
#else
				var minLogLevelForFileTarget = LogLevel.Info;
#endif
				configuration.AddTarget(minLogLevelForFileTarget, LogLevel.Fatal, FileStreamingTarget.Instance);

				configuration.IsEnabled = true;

				LogManagerFactory.DefaultConfiguration = configuration;

				isSetUp = true;
			}
		}
	}
}