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
                ExampleKeyTypeEnum.OrderId,
                id.ToString(),
                null,
                null,
                "Did you see that interesting thing, the account id was retrieved using the function? {InterestingString}", interesting);
        }
    }
}