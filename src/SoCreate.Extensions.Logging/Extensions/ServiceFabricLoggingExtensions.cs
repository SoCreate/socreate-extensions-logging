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
            public const string ServiceName = "SF.ServiceName";
            public const string ServiceTypeName = "SF.ServiceTypeName";
            public const string PartitionId = "SF.PartitionId";
            public const string ApplicationName = "SF.ApplicationName";
            public const string ApplicationTypeName = "SF.ApplicationTypeName";
            public const string NodeName = "SF.NodeName";
            public const string InstanceId = "SF.InstanceId";
            public const string ReplicaId = "SF.ReplicaId";
            public const string ServicePackageVersion = "SF.ServicePackageVersion";
        }
    }
}