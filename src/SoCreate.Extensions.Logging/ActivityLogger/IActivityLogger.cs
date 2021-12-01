using System;

namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger<in TKeyType> where TKeyType : Enum
    {
        void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            TKeyType keyType,
            string keyId,
            string? accountId,
            object? additionalData,
            string message,
            params object[] messageData);

        void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            string? accountId,
            object? additionalData,
            string message,
            params object[] messageData);
    }

    public interface IActivityLogger<in TKeyType, out TSourceContext> : IActivityLogger<TKeyType> where TKeyType : Enum
    {
    }
}