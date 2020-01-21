using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerExtensions
    {
        public static void LogActivity<TActivityEnum>(this IActivityLogger activityLogger, int key,
            TActivityEnum keyType, string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(key, keyType, null, message, messageData);
        }
    }
}