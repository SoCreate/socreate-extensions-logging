using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using SoCreate.Extensions.Logging.Exceptions;
using SoCreate.Extensions.Logging.Options;

namespace SoCreate.Extensions.Logging.LogAdapters
{
    public class ActivityLoggerLogConfigurationAdapter
    {
        public const string LogTypeKey = "LogType";
        private readonly IConfiguration _configuration;

        public ActivityLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration, ActivityLoggerOptions activityLoggerOptions)
        {
            var sqlConnectionString = string.IsNullOrEmpty(activityLoggerOptions.SqlServer?.ConnectionString)
                ? _configuration.GetValue<string>("Infrastructure:ConnectionString")
                : activityLoggerOptions.SqlServer?.ConnectionString;
            var tableName = string.IsNullOrEmpty(activityLoggerOptions.SqlServer?.TableName)
                ? "Activity"
                : activityLoggerOptions.SqlServer?.TableName;
            var schemaName = string.IsNullOrEmpty(activityLoggerOptions.SqlServer?.SchemaName)
                ? "Logging"
                : activityLoggerOptions.SqlServer?.SchemaName;
            var logType = activityLoggerOptions.ActivityLogType;

            if (sqlConnectionString == string.Empty)
            {
                throw new ActivityLoggerConnectionException(
                    "The sql connection string is not set. Either set in configuration or setup a secret for TYPE--Infrastructure--ConnectionString.");
            }

            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Remove(StandardColumn.Exception);
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn { ColumnName = "ActivityType", DataType = SqlDbType.VarChar, DataLength = 256 },
                new SqlColumn { ColumnName = "KeyType", DataType = SqlDbType.VarChar, DataLength = 64, AllowNull = true },
                new SqlColumn { ColumnName = "KeyId", DataType = SqlDbType.VarChar, DataLength = 36, NonClusteredIndex = true, AllowNull = true },
                new SqlColumn { ColumnName = "AccountId", DataType = SqlDbType.VarChar, DataLength = 20, NonClusteredIndex = true, AllowNull = true },
                new SqlColumn { ColumnName = "TenantId", DataType = SqlDbType.Int, NonClusteredIndex = true },
                new SqlColumn { ColumnName = "ProfileId", DataType = SqlDbType.Int, NonClusteredIndex = true },
                new SqlColumn { ColumnName = "Version", DataType = SqlDbType.VarChar, DataLength = 10 },
            };

            try
            {
                var sinkOptions = new MSSqlServerSinkOptions
                {
                    SchemaName = schemaName,
                    TableName = tableName,
                    AutoCreateSqlTable = false
                };

                return loggerConfiguration.AuditTo.Logger(cc =>
                      cc.Filter.ByIncludingOnly(WithProperty(LogTypeKey, logType))
                        .Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Information)
                        .AuditTo.MSSqlServer(sqlConnectionString, sinkOptions, columnOptions: columnOptions));
            }
            catch (Exception exception)
            {
                var message = $"There was an issue connecting to SqlServer Db: {exception.Message}";

                throw new Exception(message, exception);
            }
        }

        private Func<LogEvent, bool> WithProperty(string propertyName, object scalarValue)
        {
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