using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using ILogger = Serilog.ILogger;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    class ActivityLogger<TSourceContext> : IActivityLogger<TSourceContext>
    {
        private readonly ILogger _logger;
        private readonly string _activityLogType;
        private readonly string _version;

        public ActivityLogger(ILoggerProvider loggerProvider, IConfiguration configuration)
        {
            _logger = ((LoggerProvider)loggerProvider).Logger;
            _activityLogType = configuration.GetValue<string>("ActivityLogger:ActivityLogType") ?? "DefaultType";
            _version = configuration.GetValue<string>("ActivityLogger:ActivityLogVersion") ?? "1.0.0";
        }

        public void LogActivity<TActivityEnum>(
            int key, 
            TActivityEnum keyType,
            int? accountId, 
            int tenantId,
            AdditionalData? additionalData, 
            string message, 
            params object[] messageData)
        {
            if (keyType == null)
            {
                throw new ArgumentNullException(nameof(keyType), "keyType must be set");
            }
            var properties = new List<ILogEventEnricher>
            {
                new PropertyEnricher(Constants.SourceContextPropertyName, typeof(TSourceContext)),
                new PropertyEnricher("Version", _version),
                new PropertyEnricher("Key", key.ToString()),
                new PropertyEnricher("KeyType", keyType.ToString(), true),
                new PropertyEnricher("TenantId", tenantId),
                new PropertyEnricher(SqlServerLoggerLogConfigurationAdapter.LogTypeKey, _activityLogType)
            };
            
            if (additionalData != null)
            {
                properties.Add(new PropertyEnricher("AdditionalProperties", additionalData.Properties, true));
            }

            if (accountId != null)
            {
                properties.Add(new PropertyEnricher("AccountId", accountId));
            }

            using (LogContext.Push(properties.ToArray()))
            {
                _logger.Information(message, messageData);
            }
        }
    }
}