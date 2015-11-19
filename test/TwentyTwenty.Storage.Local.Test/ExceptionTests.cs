using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Blob_Deleted_Directory_Exception()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                _provider.DeleteBlob("asdf", "asdf.txt");
            });

            Assert.Equal(ex.ErrorCode, (int)StorageErrorCode.InvalidContainerName);
        }

        [Fact]
        public void Test_Get_Blob_Stream_FileNotFound_Exception()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                var container = GetRandomContainerName();
                var blobName = GenerateRandomName();
                var data = GenerateRandomBlobStream();

                CreateNewFile(container, blobName, data);

                _provider.GetBlobStream(container, "asdf.txt");
            });

            Assert.Equal(ex.ErrorCode, (int)StorageErrorCode.InvalidBlobName);
        }

        [Fact]
        public void Test_Get_Blob_Stream_Directory_Exception()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                _provider.GetBlobStream("asdf", "asdf.txt");
            });

            Assert.Equal(ex.ErrorCode, (int)StorageErrorCode.InvalidContainerName);
        }
    }
}