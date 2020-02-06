using System;

namespace SoCreate.Extensions.Logging.Options
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
    }
}