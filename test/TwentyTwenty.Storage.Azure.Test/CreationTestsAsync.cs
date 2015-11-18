using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public class CreationTestsAsync : BlobTestBase
    {
        public CreationTestsAsync(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Fact_Container_Created_Async()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetContainerReference(containerName).ExistsAsync());
            
            var data = GenerateRandomBlobStream();
            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream());

            Assert.True(await _client.GetContainerReference(containerName).ExistsAsync());
        }

        [Fact]
        public async void Test_Container_Created_Private_Async()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetContainerReference(containerName).ExistsAsync());

            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream());

            var container = _client.GetContainerReference(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Off, (await container.GetPermissionsAsync()).PublicAccess);

            containerName = GetRandomContainerName();
            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Private });

            container = _client.GetContainerReference(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Off, (await container.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Container_Created_Public_Async()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetContainerReference(containerName).ExistsAsync());

            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Public });

            var container = _client.GetContainerReference(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Blob, (await container.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Blob_Created_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data);
            await _client.GetContainerReference(container)
                .GetBlockBlobReference(blobName)
                .DownloadToStreamAsync(stream);
            
            Assert.True(StreamEquals(data, stream));
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

            var blob = _client.GetContainerReference(container)
                .GetBlockBlobReference(blobName);
            await blob.FetchAttributesAsync();
            
            Assert.Equal(dataLength, blob.Properties.Length);
            Assert.Equal(contentType, blob.Properties.ContentType);
        }
    }
}