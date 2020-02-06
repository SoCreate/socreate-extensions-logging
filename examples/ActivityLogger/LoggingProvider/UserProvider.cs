using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace ActivityLogger.LoggingProvider
{
    public class UserProvider : IUserProvider
    {
        public int GetUserIdFromRequestContext()
        {
            // Hardcoded as an example
            return 1;
        }
    }
}