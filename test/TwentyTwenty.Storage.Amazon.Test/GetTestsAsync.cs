using System.IO;
using System.Net;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class GetTestsAsync : BlobTestBase
    {
        public GetTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        private WebClient _webClient = new WebClient();

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            CreateNewObject(container, blobName, data);

            var blobStream = await _provider.GetBlobStreamAsync(container, blobName);
            var amzStream = new MemoryStream();
            blobStream.CopyTo(amzStream);
            
            Assert.True(StreamEquals(amzStream, stream));
        }

        [Fact]
        public async void Test_Get_Blob_Descriptor_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);
            var contentType = "image/png";

            CreateNewObject(container, blobName, data, true, contentType);

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

            Assert.Equal(descriptor.Container, container);
            Assert.NotEmpty(descriptor.ContentMD5);
            Assert.Equal(descriptor.ContentType, contentType);
            Assert.NotEmpty(descriptor.ETag);
            Assert.NotNull(descriptor.LastModified);
            Assert.Equal(descriptor.Length, datalength);
            Assert.Equal(descriptor.Name, blobName);
            Assert.Equal(descriptor.Security, BlobSecurity.Public);
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();

            CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/png");
            CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/jpg");
            CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "text/plain");

            foreach (var blob in await _provider.ListBlobsAsync(container))
            {
                var descriptor = await _provider.GetBlobDescriptorAsync(container, blob.Name);

                Assert.Equal(descriptor.Container, container);
                Assert.NotEmpty(descriptor.ContentMD5);
                Assert.Equal(descriptor.ContentType, blob.ContentType);
                Assert.NotEmpty(descriptor.ETag);
                Assert.NotNull(descriptor.LastModified);
                Assert.Equal(descriptor.Length, blob.Length);
                Assert.Equal(descriptor.Name, blob.Name);
                Assert.Equal(descriptor.Security, BlobSecurity.Private);
            }
        }
    }
}