using Microsoft.Extensions.Configuration;
using Serilog;
using SoCreate.Extensions.Logging.ApplicationInsightsLogger;

namespace SoCreate.Extensions.Logging
{
    class ApplicationInsightsLoggerLogConfigurationAdapter
    {
        private readonly IConfiguration _configuration;

        public ApplicationInsightsLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration, LoggerOptions options)
        {
            var instrumentationKey = _configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            return loggerConfiguration.WithApplicationInsights(instrumentationKey, options.GetUserId);
        }
    }
}