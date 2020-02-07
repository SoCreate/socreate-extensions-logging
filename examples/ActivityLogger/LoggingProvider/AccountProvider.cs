using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace ActivityLogger.LoggingProvider
{
    public class AccountProvider : IAccountProvider<ExampleKeyTypeEnum>
    {
        public int? GetAccountId(ExampleKeyTypeEnum keyType, int keyId)
        {
            // hardcoded as an example
            return keyType == ExampleKeyTypeEnum.NoteId ? 3 : 4;
        }
    }
}