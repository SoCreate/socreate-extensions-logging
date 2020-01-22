using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerExtensions
    {
        public static void LogActivity<TKeyType, TActivityEnum>(
            this IActivityLogger activityLogger,
            int key,
            TKeyType keyType,
            TActivityEnum activityType,
            int? accountId,
            int tenantId,
            string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(key, keyType, activityType, accountId, tenantId, null, message, messageData);
        }

        public static void LogActivity<TActivityEnum>(
            this IActivityLogger activityLogger,
            int key,
            TActivityEnum keyType,
            TActivityEnum activityType,
            int tenantId,
            string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(key, keyType, activityType, null, tenantId, null, message, messageData);
        }
    }
}