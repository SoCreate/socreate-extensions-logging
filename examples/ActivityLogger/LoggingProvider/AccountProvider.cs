using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace ActivityLogger.LoggingProvider
{
    public class AccountProvider : IAccountProvider<ExampleKeyTypeEnum>
    {
        public string? GetAccountId(ExampleKeyTypeEnum keyType, string keyId)
        {
            // hardcoded as an example
            return keyType == ExampleKeyTypeEnum.NoteId ? "DC11-VWD-WPJ" : "OC12-MBJ-P96";
        }
    }
}