using System;

namespace SoCreate.Extensions.Logging
{
    public class LoggerOptions
    {
        public LoggerOptions()
        {
            SendLogDataToApplicationInsights = true;
            SendLogActivityDataToCosmos = false;
        }

        public bool SendLogDataToApplicationInsights { get; set; }
        public bool SendLogActivityDataToCosmos { get; set; }
        public Func<int>? GetUserId { get; set; }
    }
}