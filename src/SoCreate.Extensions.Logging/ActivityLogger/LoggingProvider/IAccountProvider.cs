namespace SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider
{
    public interface IAccountProvider<TKeyType>
    {
        string? GetAccountId(TKeyType keyType, string keyId);
    }
}