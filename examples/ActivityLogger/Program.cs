using System;
using Microsoft.Extensions.Configuration;
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
                    new ExampleKeySet { SpecialExampleId = randomId },
                    ExampleActionType.Default,
                    new AdditionalData(("Extra", "Data"), ("MoreExtra", "Data2")),
                    "Logging Activity with Message: {Structure}",
                    "This is more information");

                // use the activity logger extensions
                activityLogger.LogSomeData(51, "This is the extension method");
            }
        }

        private static IHost CreateHost() =>
            new HostBuilder()
                .UseEnvironment(Environment.GetEnvironmentVariable("App_Environment"))
                .ConfigureAppConfiguration(builder => builder
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile(
                        $"appsettings.{Environment.GetEnvironmentVariable("App_Environment") ?? "Production"}.json",
                        true
                    ))
                .ConfigureLogging(builder =>
                    builder.AddServiceLogging(new LoggerOptions
                    {
                        UseActivityLogger = true,
                        UseApplicationInsights = false
                    }))
                .Build();
        
    }
}
