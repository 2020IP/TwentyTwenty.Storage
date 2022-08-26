using System.Linq;
using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class DeletionTests : BlobTestBase
    {
        public DeletionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Container_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName1 = GenerateRandomName();
            var blobName2 = GenerateRandomName();
            var data1 = GenerateRandomBlobStream();
            var data2 = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName1), null, data1);
            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName2), null, data2);

            var count = (await _client.ListObjectsAsync(Bucket, container)
                .ReadPageAsync(10)).Count();
            Assert.Equal(2, count);

            await _provider.DeleteContainerAsync(container);

            count = (await _client.ListObjectsAsync(Bucket, container)
                .ReadPageAsync(10)).Count();
            Assert.Equal(0, count);
        }

        [Fact]
        public async void Test_Blob_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            var count = (await _client.ListObjectsAsync(Bucket, container)
                .ReadPageAsync(10)).Count();

            Assert.Equal(1, count);

            await _provider.DeleteBlobAsync(container, blobName);

            count = (await _client.ListObjectsAsync(Bucket, container)
                .ReadPageAsync(10)).Count();
            Assert.Equal(0, count);
        }
    }
}