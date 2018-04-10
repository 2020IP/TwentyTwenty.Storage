using Microsoft.WindowsAzure.Storage.Blob;
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
            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            var contentType = "image/png";

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            blobRef.Properties.ContentType = contentType;
            await blobRef.UploadFromStreamAsync(data);

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

            await AssertBlobDescriptor(descriptor, blobRef);
        }

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            using (var blobStream = await _provider.GetBlobStreamAsync(container, blobName))
            {
                var ms = new MemoryStream();
                await blobStream.CopyToAsync(ms);
                Assert.True(StreamEquals(ms, data));
            }
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();

            var containerRef = _client.GetContainerReference(container);
            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);

            var blobRef = containerRef.GetBlockBlobReference(GenerateRandomName());
            blobRef.Properties.ContentType = "image/png";
            await blobRef.UploadFromStreamAsync(GenerateRandomBlobStream());

            blobRef = containerRef.GetBlockBlobReference(GenerateRandomName());
            blobRef.Properties.ContentType = "image/jpg";
            await blobRef.UploadFromStreamAsync(GenerateRandomBlobStream());

            blobRef = containerRef.GetBlockBlobReference(GenerateRandomName());
            blobRef.Properties.ContentType = "text/plain";
            await blobRef.UploadFromStreamAsync(GenerateRandomBlobStream());

            foreach (var blob in await _provider.ListBlobsAsync(container))
            {
                await AssertBlobDescriptor(blob, containerRef.GetBlockBlobReference(blob.Name));
            }
        }

        // Test for bug #12
        [Fact]
        public async void Test_Get_Big_Blob_List_Async()
        {
            var count = 150;
            var container = GetRandomContainerName();

            var containerRef = _client.GetContainerReference(container);
            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);

            for (int i = 0; i < count; i++)
            {
                var blobRef = containerRef.GetBlockBlobReference(GenerateRandomName());
                await blobRef.UploadFromStreamAsync(GenerateRandomBlobStream());
            }

            var list = await _provider.ListBlobsAsync(container);
            Assert.Equal(count, list.Count);
        }
    }
}