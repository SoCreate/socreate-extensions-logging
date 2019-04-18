using System;

namespace SoCreate.Extensions.Logging
{
    public class LoggerOptions
    {
        public LoggerOptions()
        {
            UseApplicationInsights = true;
            UseActivityLogger = false;
        }

        public bool UseApplicationInsights { get; set; }
        public bool UseActivityLogger { get; set; }
        public Func<int> GetUserId { get; set; }
    }
}