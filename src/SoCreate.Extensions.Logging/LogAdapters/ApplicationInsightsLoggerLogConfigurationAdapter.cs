using Microsoft.Extensions.Configuration;
using Serilog;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;
using SoCreate.Extensions.Logging.Extensions;

namespace SoCreate.Extensions.Logging.LogAdapters;

class ApplicationInsightsLoggerLogConfigurationAdapter
{
    private readonly IConfiguration _configuration;

    public ApplicationInsightsLoggerLogConfigurationAdapter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration, IProfileProvider? profileProvider, string serviceName)
    {
        var connectionString = _configuration.GetValue<string>("ApplicationInsights:ConnectionString");
        return loggerConfiguration.WithApplicationInsights(connectionString, serviceName, profileProvider);
    }
}