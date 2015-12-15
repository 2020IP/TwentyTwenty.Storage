using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Exception_InvalidKey_Auth()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                new GoogleStorageProvider(new GoogleProviderOptions
                {
                    Email = "aaa",
                    P12PrivateKey = "aaa",
                    Bucket = Bucket
                });
            });

            Assert.Equal(ex.ErrorCode, (int)StorageErrorCode.InvalidCredentials);

        }

        [Fact]
        public void Test_Exception_NoKey_Auth()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                new GoogleStorageProvider(new GoogleProviderOptions
                {
                    Email = "aaa",
                    P12PrivateKey = null,
                    Bucket = Bucket
                });
            });

            Assert.Equal(ex.ErrorCode, (int)StorageErrorCode.NoCredentialsProvided);

        }
    }
}