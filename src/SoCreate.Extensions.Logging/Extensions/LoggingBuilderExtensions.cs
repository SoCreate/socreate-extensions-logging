using System;
using System.Fabric;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            ServiceLoggingConfiguration serviceLoggingConfiguration)
        {
            builder.ConfigureServices(serviceLoggingConfiguration);
            if (serviceLoggingConfiguration.LoggerOptions.SendLogActivityDataToSql)
            {
                throw new Exception(
                    "If SendLogActivityDataToSql is true, then you must used the BuildServiceLogging overload with the key type.");
            }

            return builder;
        }

        public static ILoggingBuilder AddServiceLogging<TKeyType>(
            this ILoggingBuilder builder,
            ServiceLoggingConfiguration serviceLoggingConfiguration)
        {
            builder.ConfigureServices(serviceLoggingConfiguration);
            if (serviceLoggingConfiguration.LoggerOptions.SendLogActivityDataToSql)
            {
                builder.Services.Configure<ActivityLoggerOptions>(serviceLoggingConfiguration.Configuration.GetSection("ActivityLogger"));
                builder.Services.AddSingleton(typeof(IAccountProvider<TKeyType>), serviceLoggingConfiguration.AccountProvider);
                builder.Services.AddSingleton(typeof(ITenantProvider), serviceLoggingConfiguration.TenantProvider);
                builder.Services.AddSingleton(typeof(IActivityLogger<,>), typeof(ActivityLogger<,>));
            }
            return builder;
        }

        private static ILoggingBuilder ConfigureServices(
            this ILoggingBuilder builder,
            ServiceLoggingConfiguration serviceLoggingConfiguration)
        {
            ValidateServiceLoggingConfiguration(serviceLoggingConfiguration);
            builder.ClearProviders();
            if (serviceLoggingConfiguration.AddApplicationInsightsTelemetry)
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }

            var configuration = serviceLoggingConfiguration.Configuration;
            var loggerOptions = serviceLoggingConfiguration.LoggerOptions;

            if (serviceLoggingConfiguration.UserProvider != null)
            {
                builder.Services.AddSingleton(typeof(IUserProvider), serviceLoggingConfiguration.UserProvider);
            }

            builder.Services.AddSingleton(loggerOptions);
            builder.Services.Configure<LoggingMiddlewareOptions>(configuration.GetSection("Logging"));
            builder.Services.AddSingleton<LoggingLevelSwitch>();

            builder.Services.AddTransient<Action<ServiceContext>>(EnrichLoggerWithContext);
            builder.Services.AddTransient<LoggerConfiguration>(services => GetLoggerConfiguration(services, configuration));
            builder.Services.AddTransient<SqlServerLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient<ApplicationInsightsLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient(serviceProvider => JavaScriptEncoder.Default);

            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>(services => GetLoggerProvider(services, loggerOptions));
            return builder;
        }

        private static void ValidateServiceLoggingConfiguration(ServiceLoggingConfiguration serviceLoggingConfiguration)
        {
            if (serviceLoggingConfiguration.LoggerOptions.SendLogActivityDataToSql)
            {
                if (serviceLoggingConfiguration.UserProvider == null)
                {
                    throw new Exception(
                        "If SendLogActivityDataToSql is true, then the UserProvider on ServiceLoggingConfiguration is required");
                }

                if (serviceLoggingConfiguration.AccountProvider == null)
                {
                    throw new Exception(
                        "If SendLogActivityDataToSql is true, then the AccountProvider on ServiceLoggingConfiguration is required");
                }

                if (serviceLoggingConfiguration.TenantProvider == null)
                {
                    throw new Exception(
                        "If SendLogActivityDataToSql is true, then the TenantProvider on ServiceLoggingConfiguration is required");
                }
            }
        }

        private static Action<ServiceContext> EnrichLoggerWithContext(IServiceProvider serviceProvider)
        {
            return context =>
                ((LoggerProvider)serviceProvider.GetRequiredService<ILoggerProvider>()).Logger.EnrichLoggerWithContextProperties(context);
        }

        private static LoggerProvider GetLoggerProvider(IServiceProvider serviceProvider, LoggerOptions options)
        {
            var loggerConfig = serviceProvider.GetRequiredService<LoggerConfiguration>();

            if (options.SendLogDataToApplicationInsights)
            {
                var userProvider = serviceProvider.GetService<IUserProvider>();
                serviceProvider.GetRequiredService<ApplicationInsightsLoggerLogConfigurationAdapter>()
                    .ApplyConfiguration(loggerConfig, userProvider);
            }

            if (options.SendLogActivityDataToSql)
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