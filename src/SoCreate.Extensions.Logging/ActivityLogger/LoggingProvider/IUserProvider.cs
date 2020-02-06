namespace SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider
{
    public interface IUserProvider
    {
        int GetUserIdFromRequestContext();
    }
}