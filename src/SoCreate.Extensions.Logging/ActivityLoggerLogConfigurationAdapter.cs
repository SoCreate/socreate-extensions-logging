using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace SoCreate.Extensions.Logging
{
    class ActivityLoggerLogConfigurationAdapter
    {
        public const string LogTypeKey = "LogType";
        private readonly IConfiguration _configuration;

        public ActivityLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration)
        {
            var uri = new Uri(_configuration.GetValue<string>("Azure:CosmosDb:Uri"));
            var key = _configuration.GetValue<string>("Azure:CosmosDb:Key");
            var databaseName = _configuration.GetValue<string>("Azure:CosmosDb:DatabaseName") ?? "Diagnostics";
            var collectionName = _configuration.GetValue<string>("Azure:CosmosDb:CollectionName") ?? "Logs";
            var logType = _configuration.GetValue<string>("ActivityLogger:ActivityLogType");

            return loggerConfiguration.WriteTo.Logger(cc =>
                cc.Filter.ByIncludingOnly(WithProperty(LogTypeKey, logType))
                    .WriteTo.AzureDocumentDB(uri, key, databaseName, collectionName));
        }

        private Func<LogEvent, bool> WithProperty(string propertyName, object scalarValue)
        {
            if (propertyName == null) throw new ArgumentNullException($"{propertyName}");
            var scalar = new ScalarValue(scalarValue);
            return e =>
            {
                if (!e.Properties.TryGetValue(propertyName, out var propertyValue)) return false;

                if (propertyValue is StructureValue stValue)
                {
                    var value = stValue.Properties.FirstOrDefault(cc => cc.Name == "Id");
                    var result = scalar.Equals(value?.Value);
                    return result;
                }

                return propertyValue.Equals(scalar);
            };
        }
    }
}