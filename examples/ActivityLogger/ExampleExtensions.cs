using System;
using System.Collections.Generic;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public static class ExampleExtensions
    {
        public static void LogSomeData(this IActivityLogger activityLogger, int id, string interesting)
        {
            var keySet = new ExampleKeySet
            {
                SpecialExampleId = id
            };
            activityLogger.LogActivity(keySet, ExampleActionType.Default,
                new Dictionary<string, object>
                {
                    {"Time", DateTime.Now}
                }, "Did you see that interesting thing? {InterestingString}", interesting);
        }
    }
}