using System;

namespace SoCreate.Extensions.Logging
{
    public class ActivityLoggerOptions
    {
        public string ActivityLogType { get; set; } = null!;

        public string ActivityLogVersion { get; set; }  = null!;

        public int? BatchSize { get; set; } = null!;

        public SqlServerConfiguration SqlServer { get; set; } = null!;
    }
    
    public class ActivityLoggerOptions<TKeyType> : ActivityLoggerOptions
    {
        public ActivityLoggerFunctionOptions<TKeyType> ActivityLoggerFunctionOptions { get; set; } = null!;
    }

    public class ActivityLoggerFunctionOptions<TKeyType>
    {
        // KeyValue, KeyType return AccountId
        public Func<TKeyType, int, int?> GetAccountId { get; set; } = null!;

        public Func<int> GetTenantId { get; set; } = null!;
    }

    public class SqlServerConfiguration
    {
        public string ConnectionString { get; set; } = null!;

        public string TableName { get; set; } = null!;

        public string SchemaName { get; set; } = null!;
    }
}