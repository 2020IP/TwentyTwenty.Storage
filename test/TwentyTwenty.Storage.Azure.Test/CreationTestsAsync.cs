using System.IO;
using Xunit;
using System.Collections.Generic;
using Azure.Storage.Blobs.Models;

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

            Assert.False(await _client.GetBlobContainerClient(containerName).ExistsAsync());
            
            var data = GenerateRandomBlobStream();
            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream());

            Assert.True(await _client.GetBlobContainerClient(containerName).ExistsAsync());
        }

        [Fact]
        public async void Test_Container_Created_Private_Async()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetBlobContainerClient(containerName).ExistsAsync());

            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream());

            var container = _client.GetBlobContainerClient(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(PublicAccessType.None, (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess);

            containerName = GetRandomContainerName();
            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Private });

            container = _client.GetBlobContainerClient(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(PublicAccessType.None, (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess);
        }

        [Fact]
        public async void Test_Container_Created_Public_Async()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await _client.GetBlobContainerClient(containerName).ExistsAsync());

            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Public });

            var container = _client.GetBlobContainerClient(containerName);
            Assert.True(await container.ExistsAsync());
            Assert.Equal(PublicAccessType.Blob, (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess);
        }

        [Fact]
        public async void Test_Blob_Created_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data, closeStream: false);
            await _client.GetBlobContainerClient(container)
                .GetBlobClient(blobName)
                .DownloadToAsync(stream);
            
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

            var blob = _client.GetBlobContainerClient(container)
                .GetBlobClient(blobName);
                
            var blobProperties = await blob.GetPropertiesAsync();
            
            Assert.Equal(dataLength, blobProperties.Value.ContentLength);
            Assert.Equal(contentType, blobProperties.Value.ContentType);
        }

        [Fact]
        public async void Test_Blob_Created_ContentDisposition_Set_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var filename = "testFile.jpg";
            var dataLength = 256;            
            var data = GenerateRandomBlobStream(dataLength);
            var prop = new BlobProperties().WithContentDispositionFilename(filename);
            
            await _provider.SaveBlobStreamAsync(container, blobName, data, prop);

            var blob = _client.GetBlobContainerClient(container)
                .GetBlobClient(blobName);

            var blobProperties = await blob.GetPropertiesAsync();

            Assert.Equal(dataLength, blobProperties.Value.ContentLength);
            Assert.Equal($"attachment; filename=\"{filename}\"", blobProperties.Value.ContentDisposition);
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

            var b = _client.GetBlobContainerClient(container).GetBlobClient(blobName);
            var props = await b.GetPropertiesAsync();

            Assert.Equal(meta, props.Value.Metadata);
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