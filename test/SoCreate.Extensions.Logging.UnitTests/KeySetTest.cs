using SoCreate.Extensions.Logging.ActivityLogger;
using Xunit;

namespace SoCreate.Extensions.Logging.UnitTests
{
    public class KeySetTest
    {
        [Fact]
        public void TestToDictionary_ShouldReturnOnlyKeysThatAreSet()
        {
            var accountId = 234;
            var myEntityId = 745;
            var testKeySet = new TestKeySet
            {
                AccountId = accountId,
                MyEntityId = myEntityId
            };

            var dictionary = testKeySet.ToDictionary();
            Assert.Equal(2, dictionary.Count);
        }
    }

    public class TestKeySet : ActivityKeySet
    {
        public const string AccountIdKey = "AccountId";
        public const string MyEntityIdKey = "MyEntityId";

        public int? AccountId { get; set; }
        public int? MyEntityId { get; set; }
    }
}
