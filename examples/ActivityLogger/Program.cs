using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SoCreate.Extensions.Logging;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServiceCollection serviceCollection;
            LoggerBootstrapper.InitializeServiceFabricRegistration(
                (serviceName, logger, configuration) =>
                {
                    serviceCollection = new ServiceCollection();
                    serviceCollection.AddActivityLogger(typeof(ActivityLogger<>));
                    serviceCollection.AddSingleton(configuration);

                    var builder = new ContainerBuilder();
                    builder.Populate(serviceCollection);
                    var container = builder.Build();

                    var activityLogger = container.Resolve<IActivityLogger<ExampleActionType>>();

                    // use the activity logger directly
                    activityLogger.LogActivity(
                        new ExampleKeySet {SpecialExampleId = new Random((int) DateTime.Now.ToOADate()).Next()},
                        ExampleActionType.Default,
                        new Dictionary<string, object>
                        {
                            {"Extra", "Data"}
                        },
                        "Logging Activity with Message: {Structure}",
                        "This is more information");
                    
                    // use the activity logger extensions
                    activityLogger.LogSomeData(51, "This is the extension method");
                    
                    // exit because the service fabric initialization ends with a sleep
                    Environment.Exit(1);
                }, new ServiceFabricLoggerOptions
                {
                    ServiceName = "Example",
                    ServiceTypeName = "ExampleServiceType",
                    UseActivityLogger = true,
                }
            );
        }
    }
}