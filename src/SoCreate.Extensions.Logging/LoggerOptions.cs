using System;

namespace SoCreate.Extensions.Logging
{
    public class LoggerOptions
    {
        public LoggerOptions()
        {
            SendLogDataToApplicationInsights = true;
            SendLogActivityDataToSql = false;
        }

        public bool SendLogDataToApplicationInsights { get; set; }

        public bool SendLogActivityDataToSql { get; set; }

        public Func<int>? GetUserId { get; set; }
    }
}