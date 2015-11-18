using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Blob_Created_Exception()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                var container = GetRandomContainerName();
                var blobName = GenerateRandomName();
                var data = GenerateRandomBlobStream();

                _exceptionProvider.SaveBlobStream(container, blobName, data);
            });

            Assert.Equal(ex.ErrorCode, 1000);
        }
    }
}