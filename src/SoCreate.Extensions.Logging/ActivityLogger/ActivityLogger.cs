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
    class ActivityLogger<TKeyType, TSourceContext> : IActivityLogger<TKeyType, TSourceContext>
    {
        private readonly ILogger _logger;
        private readonly string _activityLogType;
        private readonly string _version;
        private readonly ActivityLoggerOptions<TKeyType> _options;

        public ActivityLogger(ILoggerProvider loggerProvider, IOptions<ActivityLoggerOptions<TKeyType>> options)
        {
            _logger = ((LoggerProvider)loggerProvider).Logger;
            _options = options.Value;
            _activityLogType = _options.ActivityLogType ?? "DefaultType";
            _version = _options.ActivityLogVersion ?? "1.0.0";
        }

        public void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            TKeyType keyType,
            int keyId,
            int? accountId,
            object additionalData,
            string message,
            params object[] messageData)
        {
            if (keyType == null)
            {
                throw new ArgumentNullException(nameof(keyType), "keyType must be set");
            }

            // call to get tenant and account id
            var tenantId = _options.ActivityLoggerFunctionOptions.GetTenantId();
            if (accountId == null)
            {
                accountId = _options.ActivityLoggerFunctionOptions.GetAccountId(keyId, keyType);
            }

            var properties = new List<ILogEventEnricher>
            {
                new PropertyEnricher(Constants.SourceContextPropertyName, typeof(TSourceContext)),
                new PropertyEnricher("Version", _version),
                new PropertyEnricher("KeyId", keyId),
                new PropertyEnricher("KeyType", keyType.ToString()),
                new PropertyEnricher("ActivityType", activityEnum!.ToString()),
                new PropertyEnricher("TenantId", tenantId),
                new PropertyEnricher(SqlServerLoggerLogConfigurationAdapter.LogTypeKey, _activityLogType)
            };

            if (additionalData != null)
            {
                properties.Add(new PropertyEnricher("AdditionalProperties", additionalData, true));
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