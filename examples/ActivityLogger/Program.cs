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
            LoggerBootstrapper.InitializeServiceFabricRegistration(
                (serviceName, configuration, addloggingToServiceContext) =>
                {
                    var host = new HostBuilder()
                        .ConfigureHostConfiguration(configHost => { configHost.AddConfiguration(configuration); })
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddActivityLogger(typeof(ActivityLogger<>));
                            services.AddSingleton(configuration);
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
                    // addloggingToServiceContext(ServiceContext);
                    
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