using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace SoCreate.Extensions.Logging
{
    public class SqlServerLoggerLogConfigurationAdapter
    {
        public const string LogTypeKey = "LogType";
        private readonly IConfiguration _configuration;

        public SqlServerLoggerLogConfigurationAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration ApplyConfiguration(LoggerConfiguration loggerConfiguration)
        {
            var sqlConnectionString = _configuration.GetValue<string>("SqlServer:ConnectionString") ??
                                      _configuration.GetValue<string>("Infrastructure:ConnectionString");
            var tableName = _configuration.GetValue<string>("SqlServer:TableName");
            var schemaName = _configuration.GetValue<string>("SqlServer:SchemaName");
            var logType = _configuration.GetValue<string>("ActivityLogger:ActivityLogType");

            var options = new ColumnOptions();
            options.Store.Remove(StandardColumn.Properties);
            options.Store.Remove(StandardColumn.Exception);
            options.Store.Add(StandardColumn.LogEvent);
            options.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn { ColumnName = "Key", DataType = SqlDbType.Int, NonClusteredIndex = true },
                new SqlColumn { ColumnName = "KeyType", DataType = SqlDbType.NVarChar, DataLength = 64 },
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
                            autoCreateSqlTable: true,
                            batchPostingLimit: 1000
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