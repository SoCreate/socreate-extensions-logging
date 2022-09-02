using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace SoCreate.Extensions.Logging;

class LoggerProvider : SerilogLoggerProvider
{
    public LoggerProvider(ILogger logger) : base(logger, true)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}