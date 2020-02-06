using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using SoCreate.Extensions.Logging.Options;

namespace SoCreate.Extensions.Logging.LogAdapters
{
    public class SqlServerLoggerLogConfigurationAdapter
    {
        public const string LogTypeKey = "LogType";
        private readonly IConfiguration _configuration;

        public SqlServerLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration, ActivityLoggerOptions activityLoggerOptions)
        {
            var sqlConnectionString = activityLoggerOptions.SqlServer.ConnectionString ??
                                      _configuration.GetValue<string>("Infrastructure:ConnectionString");
            var tableName = activityLoggerOptions.SqlServer.TableName ?? "Activity";
            var schemaName = activityLoggerOptions.SqlServer.SchemaName ?? "Logging";
            var logType = activityLoggerOptions.ActivityLogType;

            var options = new ColumnOptions();
            options.Store.Remove(StandardColumn.Properties);
            options.Store.Remove(StandardColumn.Exception);
            options.Store.Add(StandardColumn.LogEvent);
            options.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn { ColumnName = "ActivityType", DataType = SqlDbType.NVarChar, DataLength = 256 },
                new SqlColumn { ColumnName = "KeyType", DataType = SqlDbType.NVarChar, DataLength = 64, AllowNull = true },
                new SqlColumn { ColumnName = "KeyId", DataType = SqlDbType.Int, NonClusteredIndex = true, AllowNull = true },
                new SqlColumn { ColumnName = "AccountId", DataType = SqlDbType.Int, NonClusteredIndex = true, AllowNull = true },
                new SqlColumn { ColumnName = "TenantId", DataType = SqlDbType.Int, NonClusteredIndex = true },
                new SqlColumn { ColumnName = "UserId", DataType = SqlDbType.Int, NonClusteredIndex = true },
                new SqlColumn { ColumnName = "Version", DataType = SqlDbType.NVarChar, DataLength = 10 },
            };

            try
            {
                return loggerConfiguration.WriteTo.Logger(cc =>
                    cc.Filter.ByIncludingOnly(WithProperty(LogTypeKey, logType))
                        .WriteTo.MSSqlServer(
                            connectionString: sqlConnectionString,
                            tableName: tableName,
                            schemaName: schemaName,
                            columnOptions: options,
                            batchPostingLimit: activityLoggerOptions.BatchSize ?? 50
                        ));
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