﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using SoCreate.Extensions.Logging.ActivityLogger;
using SoCreate.Extensions.Logging.ServiceFabric;
using System;
using System.Fabric;

namespace SoCreate.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddServiceLogging(this ILoggingBuilder builder, LoggerOptions options = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            options = options ?? new LoggerOptions();

            if (options.UseActivityLogger)
            {
                builder.Services.AddSingleton(typeof(IActivityLogger<>), typeof(ActivityLogger<>));
            }

            builder.Services.AddSingleton<LoggingLevelSwitch>();
        
            builder.Services.AddTransient<Action<ServiceContext>>(serviceProvider => EnrichLoggerWithContext(serviceProvider));
            builder.Services.AddTransient<LoggerConfiguration>(services => GetLoggerConfiguration(services));
            builder.Services.AddTransient<ActivityLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient<ApplicationInsightsLoggerLogConfigurationAdapter>();

            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>(services => GetLoggerProvider(services, options));
            builder.AddFilter<LoggerProvider>(null, LogLevel.Trace);

            return builder;
        }

        private static Action<ServiceContext> EnrichLoggerWithContext(IServiceProvider serviceProvider)
        {
            return context => serviceProvider.GetRequiredService<LoggerProvider>().Logger.EnrichLoggerWithContextProperties(context);
        }

        private static LoggerProvider GetLoggerProvider(IServiceProvider serviceProvider, LoggerOptions options)
        {
            var loggerConfig = serviceProvider.GetRequiredService<LoggerConfiguration>();

            if (options.UseApplicationInsights)
            {
                serviceProvider.GetRequiredService<ApplicationInsightsLoggerLogConfigurationAdapter>()
                    .ApplyConfiguration(loggerConfig, options);
            }

            if (options.UseActivityLogger)
            {
                serviceProvider.GetRequiredService<ActivityLoggerLogConfigurationAdapter>()
                    .ApplyConfiguration(loggerConfig);
            }
            
            return new LoggerProvider(loggerConfig.CreateLogger());
        }

        private static LoggerConfiguration GetLoggerConfiguration(IServiceProvider serviceProvider)
        {
            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(serviceProvider.GetRequiredService<LoggingLevelSwitch>())
                .Enrich.FromLogContext()
                .WriteTo.Console();
        }
    }
}
