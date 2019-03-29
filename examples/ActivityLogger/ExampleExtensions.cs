using System;
using System.Collections.Generic;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    public static class ExampleExtensions
    {
        public static void LogSomeData(this IActivityLogger activityLogger, int id, string interesting)
        {
            activityLogger.LogActivity(new ExampleKeySet {SpecialExampleId = id}, ExampleActionType.Default,
                new AdditionalData(("Time", DateTime.Now)), "Did you see that interesting thing? {InterestingString}",
                interesting);
        }
    }
}