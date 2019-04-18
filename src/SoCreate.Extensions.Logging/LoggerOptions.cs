namespace SoCreate.Extensions.Logging
{
    public class LoggerOptions
    {
        public LoggerOptions()
        {
            UseApplicationInsights = true;
            UseActivityLogger = true;
        }

        public bool UseApplicationInsights { get; set; }
        public bool UseActivityLogger { get; set; }
    }
}