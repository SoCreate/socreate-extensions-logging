using System;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public static class ExampleExtensions
    {
        public static void LogSomeData<TContext>(
            this IActivityLogger<ExampleKeyTypeEnum, TContext> activityLogger,
            int id,
            string interesting)
        {
            activityLogger.LogActivity(
                ExampleActionType.GetOrder,
                id,
                ExampleKeyTypeEnum.OrderId,
                null,
                "Did you see that interesting thing, the account id was retrieved using the function? {InterestingString}",
                interesting);
        }
    }
}