namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger<in TKeyType>
    {
        void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            TKeyType keyType,
            int keyId,
            int? accountId,
            object additionalData,
            string message,
            params object[] messageData);
    }

    public interface IActivityLogger<in TKeyType, out TSourceContext> : IActivityLogger<TKeyType>
    {
    }
}