using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly ActivityLoggerOptions _options;

        public ActivityLogger(ILoggerProvider loggerProvider, IOptions<ActivityLoggerOptions> options)
        {
            _logger = ((LoggerProvider)loggerProvider).Logger;
            _options = options.Value;
            _activityLogType = _options.ActivityLogType ?? "DefaultType";
            _version = _options.ActivityLogVersion ?? "1.0.0";
            
        }

        public void LogActivity<TActivityEnum, TKeyType>(
            TActivityEnum activityEnum,
            int key,
            TKeyType keyType,
            int? accountId,
            AdditionalData additionalData,
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
                new PropertyEnricher("KeyType", keyType.ToString()),
                new PropertyEnricher("ActivityType", activityEnum.ToString()),
                new PropertyEnricher("TenantId", _options.ActivityLoggerFunctionOptions.GetTenantId()),
                new PropertyEnricher(SqlServerLoggerLogConfigurationAdapter.LogTypeKey, _activityLogType)
            };

            if (additionalData != null)
            {
                properties.Add(new PropertyEnricher("AdditionalProperties", additionalData.Properties, true));
            }

            var acctId = _options.ActivityLoggerFunctionOptions.GetAccountId(key, keyType.ToString(), accountId);
            if (acctId != null)
            {
                properties.Add(new PropertyEnricher("AccountId", acctId));
            }

            using (LogContext.Push(properties.ToArray()))
            {
                _logger.Information(message, messageData);
            }
        }
    }
}