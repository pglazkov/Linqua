using System;
using System.Collections.Generic;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
	[DefaultExport(typeof(ITelemetryService))]
	public class DefaultTelemetryService : ITelemetryService
	{
		public void TrackTrace(string message)
		{
			
		}

		public void TrackTrace(string message, TelemetrySeverityLevel severityLevel)
		{
			
		}

		public void TrackTrace(string message, IDictionary<string, string> properties)
		{
			
		}

		public void TrackTrace(string message, TelemetrySeverityLevel severityLevel, IDictionary<string, string> properties)
		{
			
		}

		public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
		{
			
		}

		public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
		{
			
		}

		public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
		{
			
		}

		public void TrackPageView(string name)
		{
			
		}

		public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
		{
			
		}
	}
}