using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                var activityLogger = host.Services.GetService<IActivityLogger<ExampleActionType>>();

                var randomId = new Random((int)DateTime.Now.ToOADate()).Next();
                // use the activity logger directly
                activityLogger.LogActivity(
                    randomId,
                    ExampleKeyTypeEnum.OrderId,
                    ExampleActionType.Important,
                    1,
                    100,
                    new AdditionalData(("Extra", "Data"), ("MoreExtra", "Data2")),
                    "Logging Activity with Message: {Structure}",
                    "This is more information");

                // use the activity logger extensions
                activityLogger.LogSomeData(51, "This is the extension method");
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
                        }));
                    config.UseStartup<Startup>();
                })
                .Build();
    }
}