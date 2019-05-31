using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace SoCreate.Extensions.Logging.ApplicationInsightsLogger
{
    public class RemoveDuplicateExceptionLogsProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public RemoveDuplicateExceptionLogsProcessor(ITelemetryProcessor next)
        {
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            if (item is ExceptionTelemetry)
            {
                // We do not want exceptions from the application insights telemetry, they will instead be logged by serilogger
                return;
            }

            Next.Process(item);
        }
    }
}
