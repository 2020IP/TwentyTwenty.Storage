using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public class CreationTests : BlobTestBase
    {
        public CreationTests(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Test_Container_Created()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetContainerReference(containerName).ExistsAsync());
            
            var data = GenerateRandomBlobStream();
            _provider.SaveBlobStream(containerName, GenerateRandomName(), GenerateRandomBlobStream());

            Assert.True(await _client.GetContainerReference(containerName).ExistsAsync());
        }

        [Fact]
        public async void Test_Container_Created_Private()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetContainerReference(containerName).ExistsAsync());

            _provider.SaveBlobStream(containerName, GenerateRandomName(), GenerateRandomBlobStream());

            var container = _client.GetContainerReference(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Off, (await container.GetPermissionsAsync()).PublicAccess);

            containerName = GetRandomContainerName();
            _provider.SaveBlobStream(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Private });

            container = _client.GetContainerReference(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Off, (await container.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Container_Created_Public()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetContainerReference(containerName).ExistsAsync());

            _provider.SaveBlobStream(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Public });

            var container = _client.GetContainerReference(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Blob, (await container.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Blob_Created()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            _provider.SaveBlobStream(container, blobName, data);
            await _client.GetContainerReference(container)
                .GetBlockBlobReference(blobName)
                .DownloadToStreamAsync(stream);

            Assert.True(StreamEquals(data, stream));
        }

        [Fact]
        public async void Test_Blob_Created_ContentType_Set()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var dataLength = 256;            
            var data = GenerateRandomBlobStream(dataLength);

            _provider.SaveBlobStream(container, blobName, data, new BlobProperties { ContentType = contentType });

            var blob = _client.GetContainerReference(container)
                .GetBlockBlobReference(blobName);
            await blob.FetchAttributesAsync();
            
            Assert.Equal(dataLength, blob.Properties.Length);
            Assert.Equal(contentType, blob.Properties.ContentType);
        }
    }
}