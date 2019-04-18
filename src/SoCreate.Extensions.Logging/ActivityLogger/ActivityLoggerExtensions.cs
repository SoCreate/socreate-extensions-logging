using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerExtensions
    {
        public static void LogActivity<TActivityEnum>(this IActivityLogger activityLogger, IActivityKeySet keySet,
            TActivityEnum actionType, string message,
            params object[] messageData)
        {
            if (activityLogger == null) throw new ArgumentNullException(nameof(activityLogger));

            activityLogger.LogActivity(keySet, actionType, null, message, messageData);
        }
    }
}