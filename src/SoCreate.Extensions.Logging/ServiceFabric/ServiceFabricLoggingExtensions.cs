using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using Serilog;
using Serilog.Core.Enrichers;

namespace SoCreate.Extensions.Logging.ServiceFabric
{
    public static class ServiceFabricLoggingExtensions
    {
        public static ILogger EnrichLoggerWithContextProperties(this ILogger logger, ServiceContext serviceContext)
        {
            var properties = new List<PropertyEnricher>
            {
                new PropertyEnricher(ServiceContextProperties.ServiceTypeName, serviceContext.ServiceTypeName),
                new PropertyEnricher(ServiceContextProperties.ServiceName, serviceContext.ServiceName),
                new PropertyEnricher(ServiceContextProperties.PartitionId, serviceContext.PartitionId),
                new PropertyEnricher(ServiceContextProperties.NodeName, serviceContext.NodeContext.NodeName),
                new PropertyEnricher(ServiceContextProperties.ApplicationName,
                    serviceContext.CodePackageActivationContext.ApplicationName),
                new PropertyEnricher(ServiceContextProperties.ApplicationTypeName,
                    serviceContext.CodePackageActivationContext.ApplicationTypeName),
                new PropertyEnricher(ServiceContextProperties.ServicePackageVersion,
                    serviceContext.CodePackageActivationContext.CodePackageVersion)
            };

            if (serviceContext is StatelessServiceContext)
            {
                properties.Add(new PropertyEnricher(ServiceContextProperties.InstanceId,
                    serviceContext.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture)));
            }
            else if (serviceContext is StatefulServiceContext)
            {
                properties.Add(new PropertyEnricher(ServiceContextProperties.ReplicaId,
                    serviceContext.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture)));
            }

            return logger.ForContext(properties);
        }

        private static class ServiceContextProperties
        {
            public const string ServiceName = "ServiceName";
            public const string ServiceTypeName = "ServiceTypeName";
            public const string PartitionId = "PartitionId";
            public const string ApplicationName = "ApplicationName";
            public const string ApplicationTypeName = "ApplicationTypeName";
            public const string NodeName = "NodeName";
            public const string InstanceId = "InstanceId";
            public const string ReplicaId = "ReplicaId";
            public const string ServicePackageVersion = "ServicePackageVersion";
        }
    }
}