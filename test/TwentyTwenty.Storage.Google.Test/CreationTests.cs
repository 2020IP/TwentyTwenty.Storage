using System.IO;
using System.Net;
using Google;
using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class CreationTests : BlobTestBase
    {
        public CreationTests(StorageFixture fixture)
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

        [Fact]
        public async void Test_Blob_Copy()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var container2 = GetRandomContainerName();
            var blobName2 = GenerateRandomName();
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            await _provider.CopyBlobAsync(container, blobName, container2, blobName2);
                        
            var blob = await _client.GetObjectAsync(Bucket, GetObjectName(container, blobName));
            Assert.NotNull(blob);
            Assert.NotEmpty(blob.MediaLink);
            Assert.Equal((ulong)dataLength, blob.Size);

            var blob2 = await _client.GetObjectAsync(Bucket, GetObjectName(container2, blobName2));
            Assert.NotNull(blob2);
            Assert.NotEmpty(blob2.MediaLink);
            Assert.Equal((ulong)dataLength, blob2.Size);
        }

        [Fact]
        public async void Test_Blob_Move()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var container2 = GetRandomContainerName();
            var blobName2 = GenerateRandomName();
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            await _provider.MoveBlobAsync(container, blobName, container2, blobName2);

            var blob = await _client.GetObjectAsync(Bucket, GetObjectName(container2, blobName2));
            Assert.NotNull(blob);
            Assert.NotEmpty(blob.MediaLink);
            Assert.Equal((ulong)dataLength, blob.Size);

            var ex = await Assert.ThrowsAsync<GoogleApiException>(() => _client.GetObjectAsync(Bucket, GetObjectName(container, blobName)));
            Assert.Equal(HttpStatusCode.NotFound, ex.HttpStatusCode);
        }
    }
}
