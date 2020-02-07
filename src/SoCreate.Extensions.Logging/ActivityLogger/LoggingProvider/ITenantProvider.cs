namespace SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider
{
    public interface ITenantProvider
    {
        int GetTenantId();
    }
}