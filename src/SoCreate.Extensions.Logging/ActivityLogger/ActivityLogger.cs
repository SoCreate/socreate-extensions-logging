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
        private readonly LoggerOptions _loggerOptions;

        public ActivityLogger(ILoggerProvider loggerProvider, 
            IOptions<ActivityLoggerOptions<TKeyType>> options,
            LoggerOptions loggerOptions)
        {
            _logger = ((LoggerProvider)loggerProvider).Logger;
            _options = options.Value;
            _activityLogType = _options.ActivityLogType ?? "DefaultType";
            _version = _options.ActivityLogVersion ?? "1.0.0";
            _loggerOptions = loggerOptions;
        }

        public void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            TKeyType keyType,
            int keyId,
            int? accountId,
            object? additionalData,
            string message,
            params object[] messageData)
        {
            if (keyType == null)
            {
                throw new ArgumentNullException(nameof(keyType), "keyType must be set");
            }

            if (accountId == null)
            {
                accountId = _options.ActivityLoggerFunctionOptions.GetAccountId(keyType, keyId);
            }
            
            Log(activityEnum, keyType.ToString(), keyId, accountId, additionalData, message, messageData);
        }

        // Log when the key type and keyid are null
        public void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            int? accountId,
            object? additionalData,
            string message,
            params object[] messageData)
        {
            Log(activityEnum, null, null, accountId, additionalData, message, messageData);
        }

        private void Log<TActivityEnum>(
            TActivityEnum activityEnum,
            string? keyType,
            int? keyId,
            int? accountId,
            object? additionalData,
            string message,
            params object[] messageData)
        {
            // call to get tenant, user id and account id
            var tenantId = _options.ActivityLoggerFunctionOptions.GetTenantId();
            var userId = _loggerOptions.GetUserId!();

            var properties = new List<ILogEventEnricher>
            {
                new PropertyEnricher(Constants.SourceContextPropertyName, typeof(TSourceContext)),
                new PropertyEnricher("Version", _version),
                new PropertyEnricher("ActivityType", activityEnum!.ToString()),
                new PropertyEnricher("TenantId", tenantId),
                new PropertyEnricher("UserId", userId),
                new PropertyEnricher(SqlServerLoggerLogConfigurationAdapter.LogTypeKey, _activityLogType)
            };

            if (keyId != null)
            {
                properties.Add(new PropertyEnricher("KeyId", keyId));
            }
            
            if (keyType != null)
            {
                properties.Add(new PropertyEnricher("keyType", keyType));
            }

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
