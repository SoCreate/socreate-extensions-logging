using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using Serilog.ExtensionMethods;

namespace SoCreate.Extensions.Logging.ApplicationInsightsLogger
{
    static class ApplicationInsightsLoggerExtensions
    {
        static Func<int> GetUserIdFromContext { get; set; }

        public static LoggerConfiguration WithApplicationInsights(this LoggerConfiguration config,
            string instrumentationKey, Func<int> getUserId = null)
        {
            if (getUserId != null)
            {
                GetUserIdFromContext = getUserId;
            }

            var telemetryConfig = new TelemetryConfiguration(instrumentationKey);
            config.WriteTo.ApplicationInsights(telemetryConfig, ConvertLogEventsToTelemetry);
            return config;
        }

        private static ITelemetry ConvertLogEventsToTelemetry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var telemetry = GetTelemetry(logEvent, formatProvider);

            // Add Operation Id
            if (Activity.Current?.RootId != null)
            {
                telemetry.Context.Operation.Id = Activity.Current?.RootId;
            }

            if (GetUserIdFromContext != null)
            {
                telemetry.Context.User.Id = GetUserIdFromContext().ToString();
            }

            return telemetry;
        }

        private static ITelemetry GetTelemetry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent.Exception != null)
            {
                // Exception telemetry
                return logEvent.ToDefaultExceptionTelemetry(
                    formatProvider,
                    includeLogLevelAsProperty: false,
                    includeRenderedMessageAsProperty: false,
                    includeMessageTemplateAsProperty: false);
            }

            // Default telemetry
            return logEvent.ToDefaultTraceTelemetry(
                formatProvider,
                includeLogLevelAsProperty: false,
                includeRenderedMessageAsProperty: false,
                includeMessageTemplateAsProperty: false);
        }
    }
}