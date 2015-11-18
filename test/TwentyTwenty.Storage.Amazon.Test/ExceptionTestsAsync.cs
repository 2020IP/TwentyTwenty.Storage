using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class ExceptionTestsAsync : BlobTestBase
    {
        public ExceptionTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Blob_Created_Exception_Async()
        {
            var ex = Assert.ThrowsAsync<StorageException>(async () =>
            {
                var container = GetRandomContainerName();
                var blobName = GenerateRandomName();
                var data = GenerateRandomBlobStream();

                await _exceptionProvider.SaveBlobStreamAsync(container, blobName, data);
            });

            Assert.Equal(ex.Result.ErrorCode, 1000);
        }
    }
}