namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger
    {
        void LogActivity<TActivityEnum>(
            int key,
            TActivityEnum keyType,
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