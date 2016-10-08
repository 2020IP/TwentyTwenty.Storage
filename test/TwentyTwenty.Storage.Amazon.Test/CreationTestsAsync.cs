using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class CreationTestsAsync : BlobTestBase
    {
        public CreationTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Created_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            await _provider.SaveBlobStreamAsync(container, blobName, data);

            var amzObject = await _client.GetObjectAsync(Bucket, container + "/" + blobName, null);

            var amzStream = new MemoryStream();
            amzObject.ResponseStream.CopyTo(amzStream);

            Assert.True(StreamEquals(amzStream, stream));
        }

        [Fact, Trait("Category", "Long")]
        public async void Test_Blob_Created_Multipart_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream(100000000);
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            await _provider.SaveBlobStreamAsync(container, blobName, data);

            var amzObject = await _client.GetObjectAsync(Bucket, container + "/" + blobName, null);

            var amzStream = new MemoryStream();
            amzObject.ResponseStream.CopyTo(amzStream);

            Assert.True(StreamEquals(amzStream, stream));
        }

        [Fact]
        public async void Test_Blob_Created_ContentType_Set_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            await _provider.SaveBlobStreamAsync(container, blobName, data, 
                new BlobProperties { ContentType = contentType });

            var amzObject = await _client.GetObjectAsync(Bucket, container + "/" + blobName, null);

            Assert.Equal(amzObject.ContentLength, dataLength);
            Assert.Equal(amzObject.Headers.ContentType, contentType);
        }
    }
}