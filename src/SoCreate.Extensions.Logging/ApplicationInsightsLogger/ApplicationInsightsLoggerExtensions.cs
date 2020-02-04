using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace SoCreate.Extensions.Logging.ApplicationInsightsLogger
{
    static class ApplicationInsightsLoggerExtensions
    {
        public static LoggerConfiguration WithApplicationInsights(this LoggerConfiguration config,
            string instrumentationKey, Func<int>? getUserId = null)
        {
            config.WriteTo.ApplicationInsights(instrumentationKey, new CustomTelemetryConvertor(getUserId));
            return config;
        }
    }

    class CustomTelemetryConvertor : TraceTelemetryConverter
    {
        private readonly Func<int>? _getUserIdFromContext;

        public CustomTelemetryConvertor(Func<int>? getUserIdFromContext)
        {
            _getUserIdFromContext = getUserIdFromContext;
        }

        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
            {
                // Add Operation Id
                if (Activity.Current?.RootId != null)
                {
                    telemetry.Context.Operation.Id = Activity.Current?.RootId;
                }

                if (_getUserIdFromContext != null)
                {
                    telemetry.Context.User.Id = _getUserIdFromContext().ToString();
                }

                yield return telemetry;
            }
        }


        public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent, ISupportProperties telemetryProperties, IFormatProvider formatProvider)
        {
            base.ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                includeLogLevel: true,
                includeRenderedMessage: true,
                includeMessageTemplate: false);
        }
    }
}
