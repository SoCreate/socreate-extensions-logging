using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerExtensions
    {
        // Ignoring additional Data
        public static void LogActivity<TActivityEnum, TKeyType>(
            this IActivityLogger<TKeyType> activityLogger,
            TActivityEnum activityType,
            int key,
            TKeyType keyType,
            int? accountId,
            string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(activityType, key, keyType, accountId, null, message, messageData);
        }

        // Ignoring AccountId and AdditionalData
        public static void LogActivity<TActivityEnum, TKeyType>(
            this IActivityLogger<TKeyType> activityLogger,
            TActivityEnum activityType,
            int key,
            TKeyType keyType,
            string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(activityType, key, keyType, null, null, message, messageData);
        }
    }
}