namespace SoCreate.Extensions.Logging.ActivityLogger
{
    public interface IActivityLogger
    {
        void LogActivity<TActivityEnum>(int key, TActivityEnum keyType,
            AdditionalData? additionalData, string message,
            params object[] messageData);
    }

    public interface IActivityLogger<out TSourceContext> : IActivityLogger
    {
    }
}