using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SoCreate.Extensions.Logging;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            LoggerBootstrapper.InitializeServiceFabricRegistration(
                (serviceName, configuration, addServiceContextToLogging) =>
                {
                    var host = new HostBuilder()
                        .ConfigureHostConfiguration(configHost => { configHost.AddConfiguration(configuration); })
                        .ConfigureServices((hostContext, services) =>
                        {
                            // Add your type of implemented Activity Logger if you extended it or implemented directly
                            // against the interface.
                            services.AddActivityLogger(typeof(ActivityLogger<>));
                        }).Build();

                    var activityLogger = host.Services.GetService<IActivityLogger<ExampleActionType>>();

                    var randomId = new Random((int) DateTime.Now.ToOADate()).Next();
                    // use the activity logger directly
                    activityLogger.LogActivity(
                        new ExampleKeySet {SpecialExampleId = randomId},
                        ExampleActionType.Default,
                        new AdditionalData(("Extra", "Data"), ("MoreExtra", "Data2")),
                        "Logging Activity with Message: {Structure}",
                        "This is more information");

                    // use the activity logger extensions
                    activityLogger.LogSomeData(51, "This is the extension method");

                    // if you had the service fabric context
                    // addServiceContextToLogging(ServiceContext);

                    // Flush for this example, normally handled in the InitializeServiceFabricRegistration function
                    Log.CloseAndFlush();
                    // exit because the service fabric initialization ends with a sleep
                    Environment.Exit(1);
                }, new ServiceFabricLoggerOptions
                {
                    ServiceName = "Example",
                    ServiceTypeName = "ExampleServiceType",
                    UseActivityLogger = true
                }
            );
        }
    }
}