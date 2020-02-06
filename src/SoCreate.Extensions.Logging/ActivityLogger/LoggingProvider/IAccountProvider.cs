namespace SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider
{
    public interface IAccountProvider<TKeyType>
    {
        int? GetAccountIdFromKeyType(TKeyType keyType, int keyId);
    }
}