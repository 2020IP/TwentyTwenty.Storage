using Azure.Storage.Blobs.Models;
using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public sealed class GetTests : BlobTestBase
    {
        public GetTests(StorageFixture fixture)
            : base(fixture)
        { }


        [Fact]
        public async void Test_Get_Blob_Descriptor_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream(256);
            var containerRef = _client.GetBlobContainerClient(container);
            var blobRef = containerRef.GetBlobClient(blobName);
            var contentType = "image/png";

            await containerRef.CreateAsync(PublicAccessType.Blob, null, null);
            
            await blobRef.UploadAsync(data, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders {
                ContentType = contentType
            }});

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

            await AssertBlobDescriptor(descriptor, blobRef);
        }

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetBlobContainerClient(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlobClient(blobName)
                .UploadAsync(data);

            using var blobStream = await _provider.GetBlobStreamAsync(container, blobName);
            var ms = new MemoryStream();
            await blobStream.CopyToAsync(ms);
            Assert.True(StreamEquals(ms, data));
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();

            var containerRef = _client.GetBlobContainerClient(container);
            await containerRef.CreateAsync(PublicAccessType.Blob, null, null);

            var blobRef = containerRef.GetBlobClient(GenerateRandomName());
            await blobRef.UploadAsync(GenerateRandomBlobStream(), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/png"
                }
            });

            blobRef = containerRef.GetBlobClient(GenerateRandomName());
            await blobRef.UploadAsync(GenerateRandomBlobStream(), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/jpg"
                }
            });

            blobRef = containerRef.GetBlobClient(GenerateRandomName());
            await blobRef.UploadAsync(GenerateRandomBlobStream(), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "text/plain"
                }
            });

            foreach (var blob in await _provider.ListBlobsAsync(container))
            {
                await AssertBlobDescriptor(blob, containerRef.GetBlobClient(blob.Name));
            }
        }

        // Test for bug #12
        [Fact]
        public async void Test_Get_Big_Blob_List_Async()
        {
            var count = 150;
            var container = GetRandomContainerName();

            var containerRef = _client.GetBlobContainerClient(container);
            await containerRef.CreateAsync(PublicAccessType.Blob, null, null);

            for (int i = 0; i < count; i++)
            {
                var blobRef = containerRef.GetBlobClient(GenerateRandomName());
                await blobRef.UploadAsync(GenerateRandomBlobStream());
            }

            var list = await _provider.ListBlobsAsync(container);
            Assert.Equal(count, list.Count);
        }
    }
}