using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public static class ActivityLoggerServiceExtensions
    {
        public static IServiceCollection AddActivityLogger(this IServiceCollection serviceCollection, Type implementedActivityLogger)
        {
            serviceCollection.AddSingleton(typeof(ILogger), Log.Logger);
            serviceCollection.AddSingleton(typeof(IActivityLogger<>), implementedActivityLogger);
            return serviceCollection;
        }
    }
}