using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SoCreate.Extensions.Logging.ApplicationInsightsLogger;
using SoCreate.Extensions.Logging.ServiceFabric;
using SerilogILogger = Serilog.ILogger;

namespace SoCreate.Extensions.Logging
{
    public static class LoggerBootstrapper
    {
        public static LoggingLevelSwitch LoggingLevelSwitch { get; set; }
        public static readonly string LogTypeKey = "LogType";

        private static LoggerConfiguration GetLoggerConfiguration()
        {
            LoggingLevelSwitch = new LoggingLevelSwitch();
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            return loggerConfiguration;
        }

        public static void InitializeServiceFabricRegistration(
            Action<string, IConfiguration, Action<ServiceContext>> registerService,
            ServiceFabricLoggerOptions serviceFabricLoggerOptions)
        {
            var configuration = GetConfiguration();
            var loggerConfiguration = GetLoggerConfiguration();
            if (serviceFabricLoggerOptions.UseApplicationInsights)
            {
                var instrumentationKey = configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
                loggerConfiguration.WithApplicationInsights(instrumentationKey,
                    serviceFabricLoggerOptions.GetUserIdFromContext);
            }

            if (serviceFabricLoggerOptions.UseActivityLogger)
            {
                var uri = new Uri(configuration.GetValue<string>("Azure:CosmosDb:Uri"));
                var key = configuration.GetValue<string>("Azure:CosmosDb:Key");
                var databaseName = configuration.GetValue<string>("Azure:CosmosDb:DatabaseName") ?? "Diagnostics";
                var collectionName = configuration.GetValue<string>("Azure:CosmosDb:CollectionName") ?? "Logs";
                var logType = configuration.GetValue<string>("ActivityLogger:ActivityLogType");
                
                loggerConfiguration.WriteTo.Logger(cc =>
                    cc.Filter.ByIncludingOnly(WithProperty(LogTypeKey, logType))
                        .WriteTo.AzureDocumentDB(uri, key, databaseName, collectionName));
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            try
            {
                Log.Information($"Initializing {serviceFabricLoggerOptions.ServiceName} Service.");

                registerService(
                    serviceFabricLoggerOptions.ServiceTypeName, configuration,
                    serviceContext => { Log.Logger.EnrichLoggerWithContextProperties(serviceContext); });

                Log.Information($"{serviceFabricLoggerOptions.ServiceTypeName} service type registered.");

                // Prevents this host process from terminating so services keeps running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Log.Fatal(e, $"{serviceFabricLoggerOptions.ServiceName} Service failed to intialize.");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static Func<LogEvent, bool> WithProperty(string propertyName, object scalarValue)
        {
            if (propertyName == null) throw new ArgumentNullException($"{propertyName}");
            var scalar = new ScalarValue(scalarValue);
            return e =>
            {
                if (!e.Properties.TryGetValue(propertyName, out var propertyValue)) return false;
                
                return propertyValue == scalarValue || propertyValue.Equals(scalar);
            };
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    true
                )
                .AddEnvironmentVariables()
                .Build();
        }
    }
}