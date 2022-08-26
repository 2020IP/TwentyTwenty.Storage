using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class ExceptionTestsAsync : BlobTestBase
    {
        public ExceptionTestsAsync()
            : base() { }

        [Fact]
        public async void Test_Blob_Deleted_Directory_Exception_Async()
        {
            var ex = await Assert.ThrowsAsync<StorageException>(() =>
            {
                return _provider.DeleteBlobAsync("asdf", "asdf.txt");
            });

            Assert.Equal((int)StorageErrorCode.InvalidName, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Get_Blob_Stream_FileNotFound_Exception_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            CreateNewFile(container, blobName, data);

            var ex = await Assert.ThrowsAsync<StorageException>(() =>
            {
                return _provider.GetBlobStreamAsync(container, "asdf.txt");
            });

            Assert.Equal((int)StorageErrorCode.InvalidName, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Get_Blob_Stream_Directory_Exception_Async()
        {
            var ex = await Assert.ThrowsAsync<StorageException>(() =>
            {
                return _provider.GetBlobStreamAsync("asdf", "asdf.txt");
            });

            Assert.Equal((int)StorageErrorCode.InvalidName, ex.ErrorCode);
        }
    }
}