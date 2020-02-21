using System;
using System.Fabric;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using SoCreate.Extensions.Logging.ActivityLogger;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;
using SoCreate.Extensions.Logging.LogAdapters;
using SoCreate.Extensions.Logging.Options;

namespace SoCreate.Extensions.Logging.Extensions
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddServiceLogging(
            this ILoggingBuilder builder,
            HostBuilderContext hostBuilderContext,
            Action<ServiceLoggingConfiguration> action)
        {
            var serviceLoggingConfiguration = new ServiceLoggingConfiguration(hostBuilderContext);
            action.Invoke(serviceLoggingConfiguration);
            builder.ConfigureServices(serviceLoggingConfiguration);
            return builder;
        }
        
        public static ILoggingBuilder AddServiceLogging(
            this ILoggingBuilder builder,
            WebHostBuilderContext webHostBuilderContext,
            Action<ServiceLoggingConfiguration> action)
        {
            var serviceLoggingConfiguration = new ServiceLoggingConfiguration(webHostBuilderContext);
            action.Invoke(serviceLoggingConfiguration);
            builder.ConfigureServices(serviceLoggingConfiguration);
            return builder;
        }

        private static ILoggingBuilder ConfigureServices(
            this ILoggingBuilder builder,
            ServiceLoggingConfiguration serviceLoggingConfiguration)
        {
            builder.ClearProviders();
            if (serviceLoggingConfiguration.ApplicationInsightsTelemetry)
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }

            var configuration = serviceLoggingConfiguration.Configuration;

            if (serviceLoggingConfiguration.UserProvider != null)
            {
                builder.Services.AddSingleton(typeof(IUserProvider), serviceLoggingConfiguration.UserProvider);
            }
            
            if (serviceLoggingConfiguration.RegisterActivityLogger)
            {
                builder.Services.Configure<ActivityLoggerOptions>(configuration.GetSection("ActivityLogger"));
                builder.Services.AddSingleton(serviceLoggingConfiguration.AccountProviderType, serviceLoggingConfiguration.AccountProvider);
                builder.Services.AddSingleton(typeof(ITenantProvider), serviceLoggingConfiguration.TenantProvider);
                builder.Services.AddSingleton(typeof(IActivityLogger<,>), typeof(ActivityLogger<,>));
            }

            builder.Services.Configure<LoggingMiddlewareOptions>(configuration.GetSection("Logging"));
            builder.Services.AddSingleton<LoggingLevelSwitch>();

            builder.Services.AddTransient<Action<ServiceContext>>(EnrichLoggerWithContext);
            builder.Services.AddTransient<LoggerConfiguration>(services => GetLoggerConfiguration(services, configuration));
            builder.Services.AddTransient<SqlServerLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient<ApplicationInsightsLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient(serviceProvider => JavaScriptEncoder.Default);

            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>(services => GetLoggerProvider(services, serviceLoggingConfiguration));
            return builder;
        }


        private static Action<ServiceContext> EnrichLoggerWithContext(IServiceProvider serviceProvider)
        {
            return context =>
                ((LoggerProvider)serviceProvider.GetRequiredService<ILoggerProvider>()).Logger.EnrichLoggerWithContextProperties(context);
        }

        private static LoggerProvider GetLoggerProvider(IServiceProvider serviceProvider, ServiceLoggingConfiguration configuration)
        {
            var loggerConfig = serviceProvider.GetRequiredService<LoggerConfiguration>();

            if (configuration.LogToApplicationInsights)
            {
                var userProvider = serviceProvider.GetService<IUserProvider>();
                serviceProvider.GetRequiredService<ApplicationInsightsLoggerLogConfigurationAdapter>()
                    .ApplyConfiguration(loggerConfig, userProvider);
            }

            if (configuration.LogToActivityLogger)
            {
                var activityLoggerOptions = serviceProvider.GetService<IOptions<ActivityLoggerOptions>>();
                serviceProvider.GetRequiredService<SqlServerLoggerLogConfigurationAdapter>()
                    .ApplyConfiguration(loggerConfig, activityLoggerOptions.Value);
            }

            return new LoggerProvider(loggerConfig.CreateLogger());
        }

        private static LoggerConfiguration GetLoggerConfiguration(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.ControlledBy(serviceProvider.GetRequiredService<LoggingLevelSwitch>())
                .Enrich.FromLogContext();
        }
    }
}