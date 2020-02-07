using Microsoft.Extensions.Configuration;
using Serilog;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;
using SoCreate.Extensions.Logging.Extensions;
using SoCreate.Extensions.Logging.Options;

namespace SoCreate.Extensions.Logging.LogAdapters
{
    class ApplicationInsightsLoggerLogConfigurationAdapter
    {
        private readonly IConfiguration _configuration;

        public ApplicationInsightsLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration, IUserProvider? userProvider)
        {
            var instrumentationKey = _configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            return loggerConfiguration.WithApplicationInsights(instrumentationKey, userProvider);
        }
    }
}