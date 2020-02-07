using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace ActivityLogger.LoggingProvider
{
    public class UserProvider : IUserProvider
    {
        public int GetUserId()
        {
            // Hardcoded as an example
            return 1;
        }
    }
}