using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using SoCreate.Extensions.Logging.ActivityLogger;
using SoCreate.Extensions.Logging.ServiceFabric;
using System;
using System.Fabric;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace SoCreate.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddServiceLogging(
            this ILoggingBuilder builder,
            HostBuilderContext hostBuilderContext,
            LoggerOptions? options = null)
        {
            options ??= new LoggerOptions();
            builder.ClearProviders();
            var isWebApp = hostBuilderContext.Properties.ContainsKey("UseStartup.StartupType");
            if (isWebApp)
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }

            return builder.BuildServices(hostBuilderContext.Configuration, options);
        }

        public static ILoggingBuilder AddServiceLogging(
            this ILoggingBuilder builder,
            WebHostBuilderContext webHostBuilderContext,
            LoggerOptions? options = null)
        {
            builder.Services.AddApplicationInsightsTelemetry();
            options ??= new LoggerOptions();
            builder.ClearProviders();
            return builder.BuildServices(webHostBuilderContext.Configuration, options);
        }

        public static ILoggingBuilder AddServiceLogging<TKeyType>(
            this ILoggingBuilder builder,
            WebHostBuilderContext webHostBuilderContext,
            LoggerOptions options,
            ActivityLoggerFunctionOptions<TKeyType> activityLoggerFunctionOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddApplicationInsightsTelemetry();
            var configuration = webHostBuilderContext.Configuration;

            builder.ClearProviders();

            builder.Services.Configure<ActivityLoggerOptions>(configuration.GetSection("ActivityLogger"));
            builder.Services.Configure<ActivityLoggerOptions<TKeyType>>(configuration.GetSection("ActivityLogger"));
            builder.Services.Configure<ActivityLoggerOptions<TKeyType>>(activityLoggerOptions =>
            {
                activityLoggerOptions.ActivityLoggerFunctionOptions = activityLoggerFunctionOptions;
            });

            builder = builder.BuildServices(configuration, options, true);

            builder.Services.AddSingleton(typeof(IActivityLogger<,>), typeof(ActivityLogger<,>));

            return builder;
        }

        private static ILoggingBuilder BuildServices(
            this ILoggingBuilder builder,
            IConfiguration configuration,
            LoggerOptions options,
            bool allowSendLogActivityDataToSql = false)
        {
            if (!allowSendLogActivityDataToSql && options.SendLogActivityDataToSql)
            {
                throw new Exception("If SendLogActivityDataToSql is true, then ActivityLoggerFunctionOptions are required");
            }

            builder.Services.Configure<LoggingMiddlewareOptions>(configuration.GetSection("Logging"));
            builder.Services.AddSingleton<LoggingLevelSwitch>();

            builder.Services.AddTransient<Action<ServiceContext>>(EnrichLoggerWithContext);
            builder.Services.AddTransient<LoggerConfiguration>(services => GetLoggerConfiguration(services, configuration));
            builder.Services.AddTransient<SqlServerLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient<ApplicationInsightsLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient(serviceProvider => JavaScriptEncoder.Default);

            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>(services => GetLoggerProvider(services, options));
            return builder;
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
                serviceProvider.GetRequiredService<ApplicationInsightsLoggerLogConfigurationAdapter>()
                    .ApplyConfiguration(loggerConfig, options);
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