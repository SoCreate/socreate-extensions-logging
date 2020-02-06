using System;

namespace SoCreate.Extensions.Logging.Options
{
    public class ActivityLoggerOptions
    {
        public string ActivityLogType { get; set; } = null!;

        public string ActivityLogVersion { get; set; }  = null!;

        public int? BatchSize { get; set; } = null!;

        public SqlServerConfiguration SqlServer { get; set; } = null!;
    }

    public class SqlServerConfiguration
    {
        public string ConnectionString { get; set; } = null!;

        public string TableName { get; set; } = null!;

        public string SchemaName { get; set; } = null!;
    }
}