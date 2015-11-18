using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class ExceptionTestsAsync : BlobTestBase
    {
        public ExceptionTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Blob_Deleted_Directory_Exception_Async()
        {
            var ex = Assert.ThrowsAsync<StorageException>(async () =>
            {
                await _provider.DeleteBlobAsync("asdf", "asdf.txt");
            });

            Assert.Equal(ex.Result.ErrorCode, 1005);
        }

        [Fact]
        public void Test_Get_Blob_Stream_FileNotFound_Exception_Async()
        {
            var ex = Assert.ThrowsAsync<StorageException>(async () =>
            {
                var container = GetRandomContainerName();
                var blobName = GenerateRandomName();
                var data = GenerateRandomBlobStream();

                CreateNewFile(container, blobName, data);

                await _provider.GetBlobStreamAsync(container, "asdf.txt");
            });

            Assert.Equal(ex.Result.ErrorCode, 1004);
        }

        [Fact]
        public void Test_Get_Blob_Stream_Directory_Exception_Async()
        {
            var ex = Assert.ThrowsAsync<StorageException>(async () =>
            {
                await _provider.GetBlobStreamAsync("asdf", "asdf.txt");
            });

            Assert.Equal(ex.Result.ErrorCode, 1005);
        }
    }
}