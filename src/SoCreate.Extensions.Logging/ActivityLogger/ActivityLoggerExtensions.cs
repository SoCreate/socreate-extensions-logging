using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerExtensions
    {
        public static void LogActivity<TActivityEnum>(
            this IActivityLogger activityLogger,
            int key,
            TActivityEnum keyType,
            int? accountId,
            int tenantId,
            string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(key, keyType, accountId, tenantId, null, message, messageData);
        }
        
        public static void LogActivity<TActivityEnum>(
            this IActivityLogger activityLogger,
            int key,
            TActivityEnum keyType,
            int tenantId,
            string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(key, keyType, null, tenantId, null, message, messageData);
        }
    }
}