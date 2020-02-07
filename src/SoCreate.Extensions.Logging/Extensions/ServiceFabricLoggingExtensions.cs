using System.Fabric;
using System.Globalization;
using Serilog;
using Serilog.Context;

namespace SoCreate.Extensions.Logging.Extensions
{
    static class ServiceFabricLoggingExtensions
    {
        public static void EnrichLoggerWithContextProperties(this ILogger logger, ServiceContext serviceContext)
        {
            LogContext.PushProperty(ServiceContextProperties.ServiceTypeName, serviceContext.ServiceTypeName);
            LogContext.PushProperty(ServiceContextProperties.ServiceName, serviceContext.ServiceName);
            LogContext.PushProperty(ServiceContextProperties.PartitionId, serviceContext.PartitionId);
            LogContext.PushProperty(ServiceContextProperties.NodeName, serviceContext.NodeContext.NodeName);
            LogContext.PushProperty(ServiceContextProperties.ApplicationName, serviceContext.CodePackageActivationContext.ApplicationName);
            LogContext.PushProperty(ServiceContextProperties.ApplicationTypeName, serviceContext.CodePackageActivationContext.ApplicationTypeName);
            LogContext.PushProperty(ServiceContextProperties.ServicePackageVersion, serviceContext.CodePackageActivationContext.CodePackageVersion);

            if (serviceContext is StatelessServiceContext)
            {
                LogContext.PushProperty(ServiceContextProperties.InstanceId,
                    serviceContext.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture));
            }
            else if (serviceContext is StatefulServiceContext)
            {
                LogContext.PushProperty(ServiceContextProperties.ReplicaId,
                    serviceContext.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture));
            }
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