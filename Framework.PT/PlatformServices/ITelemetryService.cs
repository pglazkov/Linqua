using System;
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
        /// Send information about external dependency call in the application.
        /// </summary>
        /// <param name="dependencyName">External dependency name.</param><param name="commandName">Dependency call command name.</param><param name="startTime">The time when the dependency was called.</param><param name="duration">The time taken by the external dependency to handle the call.</param><param name="success">True if the dependency call was handled successfully.</param>
        void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success);

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        /// <param name="name">Name of the page.</param>
        void TrackPageView(string name);

        /// <summary>
        /// Send information about a request handled by the application.
        /// </summary>
        /// <param name="name">The request name.</param><param name="startTime">The time when the page was requested.</param><param name="duration">The time taken by the application to handle the request.</param><param name="responseCode">The response status code.</param><param name="success">True if the request was handled successfully by the application.</param>
        void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);

        /// <summary>
        /// Send infromation about a crash in the app.
        /// </summary>
        /// <param name="exception">The exception that caused the crash.</param>
        void TrackCrash(Exception exception);

        /// <summary>
        /// Flushes the in-memory buffer.
        /// </summary>
        void Flush();
    }

    public enum TelemetrySeverityLevel
    {
        Verbose,
        Information,
        Warning,
        Error,
        Critical,
    }
}