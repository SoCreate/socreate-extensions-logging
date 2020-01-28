namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger<in TKeyType>
    {
        void LogActivity<TActivityEnum>(
            TActivityEnum activityEnum,
            int key,
            TKeyType keyType,
            int? accountId,
            AdditionalData? additionalData,
            string message,
            params object[] messageData);
    }

    public interface IActivityLogger<in TKeyType, out TSourceContext> : IActivityLogger<TKeyType>
    {
    }
}