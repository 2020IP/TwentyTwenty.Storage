using Google.Apis.Auth.OAuth2;
using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Exception_NoKey_Auth()
        {
            var ex = Assert.Throws<System.ArgumentNullException>(() =>
            {
                new GoogleStorageProvider(null, new GoogleProviderOptions
                {
                    Bucket = Bucket
                });
            });
        }
    }
}