using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class GetTestsAsync : BlobTestBase
    {
        public GetTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            await CreateNewObjectAsync(container, blobName, data);

            using (var blobStream = await _provider.GetBlobStreamAsync(container, blobName))
            {
                var amzStream = new MemoryStream();
                blobStream.CopyTo(amzStream);

                Assert.True(StreamEquals(amzStream, stream));
            }
        }

        [Fact]
        public async void Test_Get_Blob_Descriptor_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);
            var contentType = "image/png";
            var meta = new Dictionary<string, string>
            {
                { "key1", "val1" },
                { "key2", "val2" },
            };

            await CreateNewObjectAsync(container, blobName, data, true, contentType, meta);

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

            Assert.Equal(descriptor.Container, container);
            Assert.NotEmpty(descriptor.ContentMD5);
            Assert.Equal(descriptor.ContentType, contentType);
            Assert.NotEmpty(descriptor.ETag);
            Assert.NotNull(descriptor.LastModified);
            Assert.Equal(descriptor.Length, datalength);
            Assert.Equal(descriptor.Name, blobName);
            Assert.Equal(descriptor.Security, BlobSecurity.Public);
            Assert.Equal(descriptor.Metadata, meta);
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();
            var meta = new Dictionary<string, string>
            {
                { "key1", "val1" },
                { "key2", "val2" },
            };

            await CreateNewObjectAsync(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/png", meta);
            await CreateNewObjectAsync(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/jpg", meta);
            await CreateNewObjectAsync(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "text/plain", meta);

            foreach (var blob in await _provider.ListBlobsAsync(container))
            {
                Assert.Equal(meta, blob.Metadata);

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

        [Fact]
        public async void Test_Get_Blob_Url()
        {
            var container = GetRandomContainerName();
            var blob = GenerateRandomName();

            await CreateNewObjectAsync(container, blob, GenerateRandomBlobStream(), false, "image/png");

            var url = _provider.GetBlobUrl(container, blob);

            System.Console.WriteLine("URL: " + url);            
        }
    }
}