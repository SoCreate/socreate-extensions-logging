namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerExtensions
    {
        public static void LogActivity<TActivityEnum>(this IActivityLogger activityLogger, IActivityKeySet keySet,
            TActivityEnum actionType, string message,
            params object[] messageData)
        {
            activityLogger.LogActivity(keySet, actionType, null, message, messageData);
        }
    }
}