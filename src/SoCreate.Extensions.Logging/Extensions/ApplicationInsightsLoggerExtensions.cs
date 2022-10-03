using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace SoCreate.Extensions.Logging.Extensions;

static class ApplicationInsightsLoggerExtensions
{
    public static LoggerConfiguration WithApplicationInsights(
        this LoggerConfiguration config,
        string connectionString,
        IProfileProvider? profileProvider = null,
        string? serviceName = null)
    {
        config.WriteTo.ApplicationInsights(connectionString, new CustomTelemetryConvertor(profileProvider, serviceName));
        return config;
    }
}

class CustomTelemetryConvertor : TraceTelemetryConverter
{
    private readonly string? _serviceName;
    private readonly Func<int>? _getProfileIdFromContext;

    public CustomTelemetryConvertor(IProfileProvider? profileProvider = null, string? serviceName = null)
    {
        _serviceName = serviceName;
        if (profileProvider != null)
        {
            _getProfileIdFromContext = profileProvider.GetProfileId;
        }
    }

    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
        {
            if (_serviceName != null)
            {
                telemetry.Context.Cloud.RoleName = _serviceName;
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