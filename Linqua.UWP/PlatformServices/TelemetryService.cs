using System;
using System.Collections.Generic;
using System.Threading;
using Framework.PlatformServices;
using Microsoft.HockeyApp;
using Newtonsoft.Json;

namespace Linqua.PlatformServices
{
    internal class TelemetryService : ITelemetryService
    {
        private static readonly ThreadLocal<HockeyClient> Client = new ThreadLocal<HockeyClient>(() => HockeyClient.Current);

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var payload = new
            {
                Properties = properties,
                Metrics = metrics
            };

            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Event, TelemetrySeverityLevel.Information, eventName, payload));
        }

        public void TrackTrace(string message)
        {
            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Trace, TelemetrySeverityLevel.Information, message, null));
        }

        public void TrackTrace(string message, TelemetrySeverityLevel severityLevel)
        {
            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Trace, severityLevel, message, null));
        }

        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Trace, TelemetrySeverityLevel.Information, message, properties));
        }

        public void TrackTrace(string message, TelemetrySeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Trace, severityLevel, message, properties));
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Metric, TelemetrySeverityLevel.Information, $"{name} = {value}", properties));
        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var payload = new
            {
                Exception = exception.ToString(),
                Properties = properties,
                Metrics = metrics
            };

            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.Exception, TelemetrySeverityLevel.Error, exception.Message, payload));
        }

        public void TrackPageView(string name)
        {
            Client.Value.TrackEvent(FormatEvent(TelemetryEventType.PageView, TelemetrySeverityLevel.Information, name, null));
        }

        private string FormatEvent(TelemetryEventType eventType, TelemetrySeverityLevel severityLevel, string eventText, object payload)
        {
            var result = $"[{eventType.ToString().ToUpper()}] {severityLevel.ToString().ToUpper()} - {eventText}";

            if (payload != null)
            {
                var payloadJson = JsonConvert.SerializeObject(payload);

                result += Environment.NewLine + Environment.NewLine + payloadJson;
            }

            return result;
        }

        private enum TelemetryEventType
        {
            Event,
            Trace,
            Metric,
            PageView,
            Exception
        }
    }
}