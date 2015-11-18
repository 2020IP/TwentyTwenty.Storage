using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    public sealed class CreationTests : BlobTestBase
    {
        public CreationTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Blob_Created()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            _provider.SaveBlobStream(container, blobName, data);

            var amzObject = _client.GetObject(Bucket, container + "/" + blobName, null);

            var amzStream = new MemoryStream();
            amzObject.ResponseStream.CopyTo(amzStream);

            Assert.True(StreamEquals(amzStream, stream));
        }

        [Fact, Trait("Category", "Long")]
        public void Test_Blob_Created_Multipart()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream(100000000);
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            _provider.SaveBlobStream(container, blobName, data);

            var amzObject = _client.GetObject(Bucket, container + "/" + blobName, null);

            var amzStream = new MemoryStream();
            amzObject.ResponseStream.CopyTo(amzStream);

            Assert.True(StreamEquals(amzStream, stream));
        }

        [Fact]
        public void Test_Blob_Created_ContentType_Set()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            _provider.SaveBlobStream(container, blobName, data, new BlobProperties { ContentType = contentType });

            var amzObject = _client.GetObject(Bucket, container + "/" + blobName, null);

            Assert.Equal(amzObject.ContentLength, dataLength);
            Assert.Equal(amzObject.Headers.ContentType, contentType);
        }
    }
}