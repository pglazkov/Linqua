﻿using System;
using System.Collections.Generic;

namespace Framework.PlatformServices
{
    public interface ITelemetryService
    {
        /// <summary>
        /// Send an <see cref="T:Microsoft.ApplicationInsights.DataContracts.EventTelemetry"/> for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// 
        /// </summary>
        /// <param name="eventName">A name for the event.</param><param name="properties">Named string values you can use to search and classify events.</param><param name="metrics">Measurements associated with this event.</param>
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        void TrackTrace(string message);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param><param name="severityLevel">Trace severity level.</param>
        void TrackTrace(string message, TelemetrySeverityLevel severityLevel);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param><param name="properties">Named string values you can use to search and classify events.</param>
        void TrackTrace(string message, IDictionary<string, string> properties);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param><param name="severityLevel">Trace severity level.</param><param name="properties">Named string values you can use to search and classify events.</param>
        void TrackTrace(string message, TelemetrySeverityLevel severityLevel, IDictionary<string, string> properties);

        /// <summary>
        /// Send a <see cref="T:Microsoft.ApplicationInsights.DataContracts.MetricTelemetry"/> for aggregation in Metric Explorer.
        /// </summary>
        /// <param name="name">Metric name.</param><param name="value">Metric value.</param><param name="properties">Named string values you can use to classify and filter metrics.</param>
        void TrackMetric(string name, double value, IDictionary<string, string> properties = null);

        /// <summary>
        /// Send an <see cref="T:Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry"/> for display in Diagnostic Search.
        /// </summary>
        /// <param name="exception">The exception to log.</param><param name="properties">Named string values you can use to classify and search for this exception.</param><param name="metrics">Additional values associated with this exception.</param>
        void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        /// <param name="name">Name of the page.</param>
        void TrackPageView(string name);
    }

    public enum TelemetrySeverityLevel
    {
        Verbose,
        Information,
        Warning,
        Error,
        Critical
    }
}