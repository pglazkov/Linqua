using System;
using System.Collections.Generic;
using System.Threading;
using Framework.PlatformServices;
using Microsoft.HockeyApp;

namespace Linqua.PlatformServices
{
    internal class TelemetryService : ITelemetryService
    {
        private static readonly ThreadLocal<IHockeyClient> Client = new ThreadLocal<IHockeyClient>(() => HockeyClient.Current);

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            Client.Value.TrackEvent(eventName, properties, metrics);
        }

        public void TrackTrace(string message)
        {
            TrackTrace(message, TelemetrySeverityLevel.Information);
        }

        public void TrackTrace(string message, TelemetrySeverityLevel severityLevel)
        {
            Client.Value.TrackTrace(message, (SeverityLevel)severityLevel);
        }

        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            TrackTrace(message, TelemetrySeverityLevel.Information, properties);
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
            Client.Value.TrackException(exception, properties);
        }

        public void TrackPageView(string name)
        {
            Client.Value.TrackPageView(name);
        }
    }
}