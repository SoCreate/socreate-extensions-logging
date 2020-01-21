using System;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public static class ExampleExtensions
    {
        public static void LogSomeData(this IActivityLogger activityLogger, int id, string interesting)
        {
            activityLogger.LogActivity(id, ExampleActionType.OrderId, 2, 100,
                new AdditionalData(("Time", DateTime.Now)), "Did you see that interesting thing? {InterestingString}",
                interesting);
        }
    }
}