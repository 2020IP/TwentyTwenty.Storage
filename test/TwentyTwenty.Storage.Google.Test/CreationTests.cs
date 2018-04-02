using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class CreationTestsAsync : BlobTestBase
    {
        public CreationTestsAsync(StorageFixture fixture)
            : base(fixture)
        { }

        [Fact]
        public async void Test_Blob_Created_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data);
            await _client.DownloadObjectAsync(Bucket, GetObjectName(container, blobName), stream);
            
            StreamEquals(data, stream);
        }

        [Fact, Trait("Category", "Long")]
        public async void Test_Blob_Created_Resumable_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream(10000000);
            var stream = new MemoryStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data);
            await _client.DownloadObjectAsync(Bucket, GetObjectName(container, blobName), stream);
            
            StreamEquals(data, stream);
        }

        [Fact]
        public async void Test_Blob_Created_ContentType_Set_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            await _provider.SaveBlobStreamAsync(container, blobName, data, new BlobProperties { ContentType = contentType });
            
            var blob = await _client.GetObjectAsync(Bucket, GetObjectName(container, blobName));
            Assert.NotNull(blob);
            Assert.NotEmpty(blob.MediaLink);
            Assert.Equal(contentType, blob.ContentType);
            Assert.Equal((ulong)dataLength, blob.Size);
        }
    }
}
