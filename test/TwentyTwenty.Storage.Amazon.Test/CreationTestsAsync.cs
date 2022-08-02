using System.Collections.Generic;
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
            data.Position = 0;
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

        [Fact]
        public async void Test_Blob_Created_Metadata_Set_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();            
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);
            var meta = new Dictionary<string, string>
            {
                { "key1", "val1" },
                { "key2", "val2" },
            };

            await _provider.SaveBlobStreamAsync(container, blobName, data, 
                new BlobProperties { Metadata = meta });

            var amzObject = await _client.GetObjectAsync(Bucket, container + "/" + blobName, null);

            Assert.Equal(amzObject.ContentLength, dataLength);
            Assert.Equal(meta, amzObject.Metadata.ToMetadata());
        }

        [Fact]
        public async void Test_Blob_Created_Stream_Close()
        {
            var container = GetRandomContainerName();            
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            await _provider.SaveBlobStreamAsync(container, GenerateRandomName(), data, closeStream: true);
            Assert.False(data.CanRead);

            data = GenerateRandomBlobStream(dataLength);
            await _provider.SaveBlobStreamAsync(container, GenerateRandomName(), data, closeStream: false);
            Assert.True(data.CanRead);
        }
    }
}