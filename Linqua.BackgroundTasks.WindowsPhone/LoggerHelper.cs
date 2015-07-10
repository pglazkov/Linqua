using Framework.Logging;
using MetroLog;
using MetroLog.Targets;

namespace Linqua.BackgroundTasks
{
	internal static class LoggerHelper
	{
		public static void SetupLogger()
		{
			var configuration = new LoggingConfiguration();
#if DEBUG
			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget(new LoggingLayout()));
#endif
			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget(new LoggingLayout()));

			configuration.IsEnabled = true;

			LogManagerFactory.DefaultConfiguration = configuration;
		}
	}
}