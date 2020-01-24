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
            LoggerOptions? options = null,
            ActivityLoggerFunctionOptions? activityLoggerFunctionOptions = null)
        {
            var isWebApp = hostBuilderContext.Properties.ContainsKey("UseStartup.StartupType");
            if (isWebApp)
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }

            return builder.AddServiceLogging(hostBuilderContext.Configuration, options, activityLoggerFunctionOptions);
        }

        public static ILoggingBuilder AddServiceLogging(
            this ILoggingBuilder builder,
            WebHostBuilderContext webHostBuilderContext,
            LoggerOptions? options = null,
            ActivityLoggerFunctionOptions? activityLoggerFunctionOptions = null)
        {
            builder.Services.AddApplicationInsightsTelemetry();
            return builder.AddServiceLogging(webHostBuilderContext.Configuration, options, activityLoggerFunctionOptions);
        }

        private static ILoggingBuilder AddServiceLogging(
            this ILoggingBuilder builder,
            IConfiguration configuration,
            LoggerOptions? options = null,
            ActivityLoggerFunctionOptions? activityLoggerFunctionOptions = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            options ??= new LoggerOptions();

            builder.ClearProviders();

            builder.Services.Configure<LoggingMiddlewareOptions>(configuration.GetSection("Logging"));

            builder.Services.AddSingleton(typeof(IActivityLogger<>), typeof(ActivityLogger<>));

            if (options.SendLogActivityDataToSql)
            {
                if (activityLoggerFunctionOptions == null)
                {
                    throw new Exception("When sending log data to sql, you must fill out the activity logger functions");
                }
                
                builder.Services.Configure<ActivityLoggerOptions>(configuration.GetSection("ActivityLogger"));
                builder.Services.PostConfigure<ActivityLoggerOptions>(activityLoggerOptions =>
                {
                    activityLoggerOptions.ActivityLoggerFunctionOptions = activityLoggerFunctionOptions;
                });
            }

            builder.Services.AddSingleton<LoggingLevelSwitch>();

            builder.Services.AddTransient<Action<ServiceContext>>(serviceProvider => EnrichLoggerWithContext(serviceProvider));
            builder.Services.AddTransient<LoggerConfiguration>(services => GetLoggerConfiguration(services, configuration));
            builder.Services.AddTransient<SqlServerLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient<ApplicationInsightsLoggerLogConfigurationAdapter>();
            builder.Services.AddTransient(serviceProvider => JavaScriptEncoder.Default);

            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>(services => GetLoggerProvider(services, options));

            return builder;
        }

        private static Action<ServiceContext> EnrichLoggerWithContext(IServiceProvider serviceProvider)
        {
            return context => ((LoggerProvider)serviceProvider.GetRequiredService<ILoggerProvider>()).Logger.EnrichLoggerWithContextProperties(context);
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