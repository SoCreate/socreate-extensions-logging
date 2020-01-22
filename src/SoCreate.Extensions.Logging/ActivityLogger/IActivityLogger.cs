namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger
    {
        void LogActivity<TKeyType, TActivityEnum>(
            int key,
            TKeyType keyType,
            TActivityEnum activityEnum,
            int? accountId,
            int tenantId,
            AdditionalData? additionalData,
            string message,
            params object[] messageData);
    }

    public interface IActivityLogger<out TSourceContext> : IActivityLogger
    {
    }
}