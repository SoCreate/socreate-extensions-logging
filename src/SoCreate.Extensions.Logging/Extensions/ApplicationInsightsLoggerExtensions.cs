using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace SoCreate.Extensions.Logging.Extensions
{
    static class ApplicationInsightsLoggerExtensions
    {
        public static LoggerConfiguration WithApplicationInsights(this LoggerConfiguration config,
            string instrumentationKey, IProfileProvider? profileProvider = null, ServiceContext? serviceContext = null)
        {
            config.WriteTo.ApplicationInsights(instrumentationKey, new CustomTelemetryConvertor(profileProvider, serviceContext));
            return config;
        }
    }

    class CustomTelemetryConvertor : TraceTelemetryConverter
    {
        private readonly ServiceContext? _serviceContext;
        private readonly Func<int>? _getProfileIdFromContext;

        public CustomTelemetryConvertor(IProfileProvider? profileProvider = null, ServiceContext? serviceContext = null)
        {
            _serviceContext = serviceContext;
            if (profileProvider != null)
            {
                _getProfileIdFromContext = profileProvider.GetProfileId;
            }
        }

        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
            {
                if (_serviceContext != null)
                {
                    telemetry.Context.Cloud.RoleName = _serviceContext.ServiceName.ToString();
                    telemetry.Context.Cloud.RoleInstance = _serviceContext.ReplicaOrInstanceId.ToString();
                    ServiceFabricLoggingExtensions.AddServiceFabricPropertiesToTelemetry(_serviceContext, telemetry.Context.GlobalProperties);
                }
                else
                {
                    if (logEvent.Properties.ContainsKey(ServiceFabricLoggingExtensions.ServiceContextProperties
                        .ServiceName))
                    {
                        telemetry.Context.Cloud.RoleName = logEvent
                            .Properties[ServiceFabricLoggingExtensions.ServiceContextProperties.ServiceName].ToString();
                    }

                    if (logEvent.Properties.ContainsKey(ServiceFabricLoggingExtensions.ServiceContextProperties
                        .InstanceId))
                    {
                        telemetry.Context.Cloud.RoleInstance = logEvent
                            .Properties[ServiceFabricLoggingExtensions.ServiceContextProperties.InstanceId].ToString();
                    }
                }

                // Add Operation Id
                if (Activity.Current?.RootId != null)
                {
                    telemetry.Context.Operation.Id = Activity.Current?.RootId;
                }

                if (_getProfileIdFromContext != null)
                {
                    telemetry.Context.User.Id = _getProfileIdFromContext().ToString();
                }

                yield return telemetry;
            }
        }


        public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent, ISupportProperties telemetryProperties, IFormatProvider formatProvider)
        {
            base.ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                includeLogLevel: false,
                includeRenderedMessage: false,
                includeMessageTemplate: false);
        }

        public override ExceptionTelemetry ToExceptionTelemetry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var exceptionTelemetry = base.ToExceptionTelemetry(logEvent, formatProvider);
            exceptionTelemetry.Properties["LogMessage"] = logEvent.RenderMessage();
            return exceptionTelemetry;
        }
    }
}
