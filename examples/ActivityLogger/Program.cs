using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoCreate.Extensions.Logging;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var host = CreateHost())
            {
                var logger = host.Services.GetService<ILogger<Program>>();
                logger.LogInformation("Testing sending the application insights logs {extra}", "with more text");

                var activityLogger = host.Services.GetService<IActivityLogger<ExampleKeyTypeEnum, Program>>();

                var orderId = new Random((int)DateTime.Now.ToOADate()).Next();

                // use the activity logger directly
                activityLogger.LogActivity(ExampleActionType.GetOrder, orderId, ExampleKeyTypeEnum.OrderId, 1,
                    new AdditionalData(("Price", "10.54"), ("ShipDate", "10-21-2019")), "Order was placed by {CustomerName} on {OrderDate}",
                    "Bill Battson", new DateTime(2019, 10, 15, 0, 0, 0));

                // not sending the account id, but letting the function do the work
                var noteId = new Random((int)DateTime.Now.ToOADate()).Next();
                activityLogger.LogActivity(ExampleActionType.GetNote,
                    noteId,
                    ExampleKeyTypeEnum.NoteId,
                    "This is without account {Key} or additional data", noteId);

                // use the activity logger extensions
                activityLogger.LogSomeData<Program>(51, "This is the extension method");
                host.Run();
            }
        }

        private static IHost CreateHost() =>
            Host.CreateDefaultBuilder()
                .UseEnvironment(Environment.GetEnvironmentVariable("App_Environment") ?? "Production")
                .ConfigureWebHostDefaults(config =>
                {
                    config.ConfigureLogging((hostingContext, builder) =>
                        builder.AddServiceLogging(hostingContext, new LoggerOptions
                            {
                                SendLogDataToApplicationInsights = true,
                                SendLogActivityDataToSql = false
                            },
                            new ActivityLoggerFunctionOptions<ExampleKeyTypeEnum>
                            {
                                GetTenantId = () => 100,
                                GetAccountId = (key, keyType) => (keyType == ExampleKeyTypeEnum.NoteId ? 3 : 4)
                            }));
                    config.UseStartup<Startup>();
                })
                .Build();
    }
}