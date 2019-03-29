﻿using System.Collections.Generic;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public class ActivityLogger<TSourceContext> : IActivityLogger<TSourceContext>
    {
        private readonly ILogger _logger;
        private readonly string _activityLogType;
        private readonly string _version;

        public ActivityLogger(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _activityLogType = configuration.GetValue<string>("ActivityLogger:ActivityLogType") ?? "DefaultType";
            _version = configuration.GetValue<string>("ActivityLogger:ActivityLogVersion") ?? "v1";
        }

        public void LogActivity<TActivityEnum>(IActivityKeySet keySet, TActivityEnum actionType, string message,
            params object[] messageData)
        {
            LogActivity(keySet, actionType, null, message, messageData);
        }

        public void LogActivity<TActivityEnum>(IActivityKeySet keySet, TActivityEnum actionType,
            AdditionalData additionalData, string message, params object[] messageData)
        {
            var properties = new List<ILogEventEnricher>
            {
                new PropertyEnricher(Constants.SourceContextPropertyName, typeof(TSourceContext)),
                new PropertyEnricher("Version", _version),
                new PropertyEnricher("KeySet", keySet.ToDictionary()),
                new PropertyEnricher("ActionType", actionType, true),
                new PropertyEnricher(LoggerBootstrapper.LogTypeKey, _activityLogType)
            };
            if (!additionalData.IsNull())
            {
                properties.Add(new PropertyEnricher("AdditionalProperties", additionalData.Dictionary, true));
            }

            using (LogContext.Push(properties.ToArray()))
            {
                _logger.Information(message, messageData);
            }
        }
    }
}