using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage;
using MetroLog;

namespace Linqua
{
	public static class ExceptionHandlingHelper
	{
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(ExceptionHandlingHelper).Name);

		public static void HandleNonFatalError(Exception exception, string errorHeader = null, bool sendTelemetry = true, [CallerMemberName] string callerMethodName = null)
		{
			try
			{
				if (Log.IsErrorEnabled)
				{
					errorHeader = errorHeader ?? "An error occured (non-fatal).";

					Log.Error($"[Caller:{callerMethodName}] {errorHeader}", exception);
				}

				if (sendTelemetry)
				{
					Telemetry.Client.TrackException(exception);
				}

				ApplicationData.Current.LocalSettings.Values[LocalSettingsKeys.LogsUploadPending] = true;
			}
			catch (Exception)
			{
#if DEBUG
				if (Debugger.IsAttached)
				{
					Debugger.Break();
				}
#endif
			}
		}
	}
}