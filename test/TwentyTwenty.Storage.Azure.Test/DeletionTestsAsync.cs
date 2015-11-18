using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public sealed class DeletionTestsAsync : BlobTestBase
    {
        public DeletionTestsAsync(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Test_Container_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetContainerReference(container);
            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName).UploadFromStreamAsync(data); // Why not create a file in there for good measure

            await _provider.DeleteContainerAsync(container);

            Assert.False(await containerRef.ExistsAsync());
        }

        [Fact]
        public async void Test_Blob_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            await containerRef.CreateAsync();
            await blobRef.UploadFromStreamAsync(data);

            await _provider.DeleteBlobAsync(container, blobName);

            Assert.False(await blobRef.ExistsAsync());
        }

        [Fact]
        public async void Test_Blob_Deleted_Without_Exception_When_NonExistant_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            await _provider.DeleteBlobAsync(container, blobName);
        }

        [Fact]
        public async void Test_Container_Deleted_Without_Exception_When_NonExistant_Async()
        {
            var container = GetRandomContainerName();
            await _provider.DeleteContainerAsync(container);
        }
    }
}