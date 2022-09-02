using ActivityLogger.LoggingProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoCreate.Extensions.Logging.ActivityLogger;
using SoCreate.Extensions.Logging.Extensions;

namespace ActivityLogger;

internal class Program
{
    public static void Main(string[] args)
    {
        using (var host = CreateHost())
        {
            var logger = host.Services.GetService<ILogger<Program>>()!;
            logger.LogInformation("Testing sending the application insights logs {extra}", "with more text");

            var activityLogger = host.Services.GetService<IActivityLogger<ExampleKeyTypeEnum, Program>>();
            if (activityLogger != null)
            {
                var orderId = new Random(DateTime.Now.Millisecond).Next();

                // use the activity logger directly
                activityLogger.LogActivity(
                    ExampleActionType.GetOrder,
                    ExampleKeyTypeEnum.OrderId,
                    orderId.ToString(),
                    "MA13-97H-DQF",
                    new { Price = "10.54", ShipDate = "10-21-2019" },
                    "Order was placed by {CustomerName} on {OrderDate}",
                    "Bill Battson",
                    new DateTime(2019, 10, 15, 0, 0, 0));

                // not sending the account id, but letting the function do the work
                var noteId = new Random(DateTime.Now.Millisecond).Next();
                activityLogger.LogActivity(
                    ExampleActionType.GetNote,
                    ExampleKeyTypeEnum.NoteId,
                    noteId.ToString(),
                    null,
                    null,
                    "This is without account {Key} or additional data",
                    noteId);

                // dont send the key type or key id
                activityLogger.LogActivity(
                    ExampleActionType.GetNote,
                    "MA13-97H-DQF",
                    null,
                    "This log does not have a key type, because we attempted to find a note and there was not one");

                // use the activity logger extensions
                activityLogger.LogSomeData<Program>(51, "This is the extension method");
            }

            try
            {
                throw new InvalidOperationException("My invalid operation");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Some Invalid Operation Error Occurred {ErrorLocation}", "Right Here");
            }

            host.Run();
        }
    }

    private static IHost CreateHost() =>
        Host.CreateDefaultBuilder()
            .UseEnvironment(Environment.GetEnvironmentVariable("App_Environment") ?? "Production")
            .ConfigureWebHostDefaults(config =>
            {
                config.ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddServiceLogging(
                        hostingContext,
                        loggingConfig =>
                        {
                            loggingConfig
                                .AddApplicationInsights(appConfig => appConfig.WithProfileProvider<ProfileProvider>());
                            // .AddActivityLogging<ExampleKeyTypeEnum>(
                            //     activityConfig =>
                            //         activityConfig
                            //             .WithAccountProvider<AccountProvider>()
                            //             .WithProfileProvider<ProfileProvider>()
                            //             .WithTenantProvider<TenantProvider>());
                        });
                });
                config.UseStartup<Startup>();
            })
            .Build();
}