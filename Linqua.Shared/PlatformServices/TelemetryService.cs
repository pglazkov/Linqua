using System;
using System.Collections.Generic;
using System.Threading;
using Framework.PlatformServices;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Linqua.Shared.PlatformServices
{
	internal class TelemetryService : ITelemetryService
    {
		private static readonly ThreadLocal<TelemetryClient> Client = new ThreadLocal<TelemetryClient>(() => new TelemetryClient());

		public void TrackTrace(string message)
	    {
		    Client.Value.TrackTrace(message);
	    }

	    public void TrackTrace(string message, TelemetrySeverityLevel severityLevel)
	    {
			Client.Value.TrackTrace(message, (SeverityLevel)severityLevel);
        }

	    public void TrackTrace(string message, IDictionary<string, string> properties)
	    {
			Client.Value.TrackTrace(message, properties);
        }

	    public void TrackTrace(string message, TelemetrySeverityLevel severityLevel, IDictionary<string, string> properties)
	    {
			Client.Value.TrackTrace(message, (SeverityLevel)severityLevel, properties);
        }

	    public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
	    {
			Client.Value.TrackMetric(name, value, properties);
        }

	    public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
	    {
			Client.Value.TrackException(exception, properties, metrics);
        }

	    public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
	    {
			Client.Value.TrackDependency(dependencyName, commandName, startTime, duration, success);
        }

	    public void TrackPageView(string name)
	    {
		    Client.Value.TrackPageView(name);
	    }

	    public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
	    {
			Client.Value.TrackRequest(name, startTime, duration, responseCode, success);
        }
    }
}
