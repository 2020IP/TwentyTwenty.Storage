using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class ExceptionTestsAsync : BlobTestBase
    {
        public ExceptionTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Created_Exception_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            var ex = await Assert.ThrowsAsync<StorageException>(() =>
            {
                return _exceptionProvider.SaveBlobStreamAsync(container, blobName, data);
            });

            Assert.Equal(ex.ErrorCode, 1000);
        }
    }
}