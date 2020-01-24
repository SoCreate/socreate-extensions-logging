using System;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public static class ExampleExtensions
    {
        public static void LogSomeData(this IActivityLogger activityLogger, int id, string interesting)
        {
            activityLogger.LogActivity(id, ExampleKeyTypeEnum.OrderId, ExampleActionType.Important, null,
                new AdditionalData(("Time", DateTime.Now)),
                "Did you see that interesting thing, the account id was retrieved using the function? {InterestingString}",
                interesting);
        }
    }
}