using Microsoft.Extensions.Logging;

namespace SoCreate.Extensions.Logging
{
    public static class LoggerExtensions
    {
        public static void LogSecurity(this ILogger logger, int errorNumber, string message)
        {
            logger.LogWarning("{LogType} - {ErrorNumber}: {Message}", "Security", errorNumber, message);
        }

        public static void LogAudit(this ILogger logger, int errorNumber, string message)
        {
            logger.LogInformation("{LogType} - {ErrorNumber}: {Message}", "Audit", errorNumber, message);
        }
    }
}
