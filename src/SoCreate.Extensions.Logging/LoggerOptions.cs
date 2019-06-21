using System;

namespace SoCreate.Extensions.Logging
{
    public class LoggerOptions
    {
        public LoggerOptions()
        {
            LogTelemetryDataToApplicationInsights = true;
            LogActivityDataToCosmos = false;
        }

        public bool LogTelemetryDataToApplicationInsights { get; set; }
        public bool LogActivityDataToCosmos { get; set; }
        public Func<int>? GetUserId { get; set; }
    }
}