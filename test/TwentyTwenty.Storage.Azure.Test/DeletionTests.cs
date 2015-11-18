using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public sealed class DeletionTests : BlobTestBase
    {
        public DeletionTests(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Test_Container_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetContainerReference(container);
            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName).UploadFromStreamAsync(data); // Why not create a file in there for good measure

            _provider.DeleteContainer(container);

            Assert.False(await containerRef.ExistsAsync());
        }

        [Fact]
        public async void Test_Blob_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            await containerRef.CreateAsync();
            await blobRef.UploadFromStreamAsync(data);

            _provider.DeleteBlob(container, blobName);

            Assert.False(await blobRef.ExistsAsync());
        }

        [Fact]
        public void Test_Blob_Deleted_Without_Exception_When_NonExistant()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            _provider.DeleteBlob(container, blobName);
        }

        [Fact]
        public void Test_Container_Deleted_Without_Exception_When_NonExistant()
        {
            var container = GetRandomContainerName();
            _provider.DeleteContainer(container);
        }
    }
}