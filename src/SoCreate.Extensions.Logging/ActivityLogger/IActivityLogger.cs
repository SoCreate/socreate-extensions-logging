namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger
    {
        void LogActivity<TActivityEnum, TKeyType>(
            TActivityEnum activityEnum,
            int key,
            TKeyType keyType,
            int? accountId,
            AdditionalData additionalData,
            string message,
            params object[] messageData);
    }

    public interface IActivityLogger<out TSourceContext> : IActivityLogger
    {
    }
}