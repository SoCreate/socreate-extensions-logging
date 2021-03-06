﻿using System.Fabric;
using Microsoft.Extensions.Configuration;
using Serilog;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;
using SoCreate.Extensions.Logging.Extensions;

namespace SoCreate.Extensions.Logging.LogAdapters
{
    class ApplicationInsightsLoggerLogConfigurationAdapter
    {
        private readonly IConfiguration _configuration;

        public ApplicationInsightsLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration, IProfileProvider? profileProvider, ServiceContext? serviceContext)
        {
            var instrumentationKey = _configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            return loggerConfiguration.WithApplicationInsights(instrumentationKey, profileProvider, serviceContext);
        }
    }
}