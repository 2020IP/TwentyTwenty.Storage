using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Xunit;
using System.Collections.Generic;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public class CopyMoveTests : BlobTestBase
    {
        public CopyMoveTests(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Test_Blob_Moved()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var containerRef = _client.GetContainerReference(sourceContainer);
            var blobRef = containerRef.GetBlockBlobReference(sourceName);
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            await blobRef.UploadFromStreamAsync(data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            await _client.GetContainerReference(destContainer)
                .GetBlockBlobReference(sourceName)
                .DownloadToStreamAsync(stream);
            
            Assert.True(StreamEquals(data, stream));

            // Make sure source no longer exists
            Assert.False(await blobRef.ExistsAsync());
        }

        [Fact]
        public async void Test_Blob_Moved_And_Renamed()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var containerRef = _client.GetContainerReference(sourceContainer);
            var blobRef = containerRef.GetBlockBlobReference(sourceName);
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            await blobRef.UploadFromStreamAsync(data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            await _client.GetContainerReference(destContainer)
                .GetBlockBlobReference(destName)
                .DownloadToStreamAsync(stream);
            
            Assert.True(StreamEquals(data, stream));

            // Make sure source no longer exists
            Assert.False(await blobRef.ExistsAsync());
        }

        [Fact]
        public async void Test_Blob_Copied()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var containerRef = _client.GetContainerReference(sourceContainer);
            var blobRef = containerRef.GetBlockBlobReference(sourceName);
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            await blobRef.UploadFromStreamAsync(data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            await _client.GetContainerReference(destContainer)
                .GetBlockBlobReference(sourceName)
                .DownloadToStreamAsync(stream);
            
            Assert.True(StreamEquals(data, stream));

            stream = new MemoryStream();

            // Make sure source still exists
            await _client.GetContainerReference(sourceContainer)
                .GetBlockBlobReference(sourceName)
                .DownloadToStreamAsync(stream);

            Assert.True(StreamEquals(data, stream));
        }

        [Fact]
        public async void Test_Blob_Copied_With_New_Name()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var containerRef = _client.GetContainerReference(sourceContainer);
            var blobRef = containerRef.GetBlockBlobReference(sourceName);
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            await blobRef.UploadFromStreamAsync(data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            await _client.GetContainerReference(destContainer)
                .GetBlockBlobReference(destName)
                .DownloadToStreamAsync(stream);
            
            Assert.True(StreamEquals(data, stream));

            stream = new MemoryStream();

            // Make sure source still exists
            await _client.GetContainerReference(sourceContainer)
                .GetBlockBlobReference(sourceName)
                .DownloadToStreamAsync(stream);

            Assert.True(StreamEquals(data, stream));
        }
    }
}