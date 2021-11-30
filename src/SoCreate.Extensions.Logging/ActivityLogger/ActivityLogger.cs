using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;
using SoCreate.Extensions.Logging.LogAdapters;
using SoCreate.Extensions.Logging.Options;
using ILogger = Serilog.ILogger;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    class ActivityLogger<TKeyType, TSourceContext> : IActivityLogger<TKeyType, TSourceContext> where TKeyType : Enum
    {
        private readonly ILogger _logger;
        private readonly string _activityLogType;
        private readonly string _version;
        private readonly IProfileProvider _profileProvider;
        private readonly IAccountProvider<TKeyType> _accountProvider;
        private readonly ITenantProvider _tenantProvider;

        public ActivityLogger(
            ILoggerProvider loggerProvider,
            IOptions<ActivityLoggerOptions> activityLoggerOptions,
            IProfileProvider profileProvider,
            IAccountProvider<TKeyType> accountProvider,
            ITenantProvider tenantProvider)
        {
            _logger = ((LoggerProvider)loggerProvider).Logger;
            _activityLogType = activityLoggerOptions.Value.ActivityLogType ?? "DefaultType";
            _version = activityLoggerOptions.Value.ActivityLogVersion ?? "1.0.0";
            _profileProvider = profileProvider;
            _accountProvider = accountProvider;
            _tenantProvider = tenantProvider;
        }

        public void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            TKeyType keyType,
            string keyId,
            string? accountId,
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
                accountId = _accountProvider.GetAccountId(keyType, keyId);
            }

            Log(activityEnum, keyType.ToString(), keyId, accountId, additionalData, message, messageData);
        }

        // Log when the key type and keyid are null
        public void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            string? accountId,
            object? additionalData,
            string message,
            params object[] messageData)
        {
            Log(activityEnum, null, null, accountId, additionalData, message, messageData);
        }

        private void Log<TActivityEnum>(
            TActivityEnum activityEnum,
            string? keyType,
            string? keyId,
            string? accountId,
            object? additionalData,
            string message,
            params object[] messageData)
        {
            // call to get tenant, profile id and account id
            var tenantId = _tenantProvider.GetTenantId();
            var profileId = _profileProvider.GetProfileId();

            var properties = new List<ILogEventEnricher>
            {
                new PropertyEnricher(Constants.SourceContextPropertyName, typeof(TSourceContext)),
                new PropertyEnricher("Version", _version),
                new PropertyEnricher("ActivityType", activityEnum!.ToString()),
                new PropertyEnricher("TenantId", tenantId),
                new PropertyEnricher("ProfileId", profileId),
                new PropertyEnricher(ActivityLoggerLogConfigurationAdapter.LogTypeKey, _activityLogType)
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

            try
            {
                using (LogContext.Push(properties.ToArray()))
                {
                    _logger.Information(message, messageData);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error activity logging: {message}", e.Message, e);
            }
        }
    }
}