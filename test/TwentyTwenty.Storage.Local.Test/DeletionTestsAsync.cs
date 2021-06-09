using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class DeletionTestsAsync : BlobTestBase
    {
        public DeletionTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Container_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            await _provider.DeleteContainerAsync(container);

            Assert.False(Directory.Exists(Path.Combine(BasePath, container)));
        }

        [Fact]
        public async void Test_Blob_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            await _provider.DeleteBlobAsync(container, blobName);

            Assert.False(File.Exists(Path.Combine(BasePath, container, blobName)));
        }
        
        [Fact]
        public async void Test_Blob_Deleted_Async_Also_Removes_Metadata()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
            {
                Security = BlobSecurity.Public
            });
            
            await _provider.DeleteBlobAsync(container, blobName);

            Assert.False(File.Exists(Path.Combine(BasePath, container, blobName)));
            Assert.False(File.Exists(Path.Combine(BasePath, container, $"{blobName}-meta.json")));
        }
    }
}