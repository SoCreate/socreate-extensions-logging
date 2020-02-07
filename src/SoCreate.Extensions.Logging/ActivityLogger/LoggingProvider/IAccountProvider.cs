namespace SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider
{
    public interface IAccountProvider<TKeyType>
    {
        int? GetAccountId(TKeyType keyType, int keyId);
    }
}