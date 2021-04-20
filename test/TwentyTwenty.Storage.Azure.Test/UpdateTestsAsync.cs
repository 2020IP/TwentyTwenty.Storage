using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public sealed class UpdateTestsAsync : BlobTestBase
    {
        public UpdateTestsAsync(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Test_Container_Permissions_Elevated_On_Save_Async()
        {
            var containerName = GetRandomContainerName();
            var containerRef = _client.GetBlobContainerClient(containerName);

            await containerRef.CreateAsync(PublicAccessType.None, null, null);

            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Public });
            
            Assert.True(await containerRef.ExistsAsync());
            Assert.Equal(PublicAccessType.Blob, (await containerRef.GetPropertiesAsync()).Value.PublicAccess);
        }

        [Fact]
        public async void Test_Container_Permissions_Elevated_On_Update_Async()
        {
            var containerName = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var containerRef = _client.GetBlobContainerClient(containerName);
            var blobRef = containerRef.GetBlobClient(blobName);
            var data = GenerateRandomBlobStream();

            await containerRef.CreateAsync(PublicAccessType.None, null, null);

            await blobRef.UploadAsync(data);
            await _provider.UpdateBlobPropertiesAsync(containerName, blobName, new BlobProperties { Security = BlobSecurity.Public });

            Assert.Equal(PublicAccessType.Blob, (await containerRef.GetPropertiesAsync()).Value.PublicAccess);
        }

        [Fact]
        public async void Test_Blob_Properties_Updated_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var contentDisposition = "attachment; filename=\"muhFile.jpg\"";
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetBlobContainerClient(container);
            var blobRef = containerRef.GetBlobClient(blobName);

            await containerRef.CreateAsync();

            await blobRef.UploadAsync(data, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/png"
                }
            });
            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties 
            { 
                ContentType = contentType,
                ContentDisposition = contentDisposition,
            });
            
            var blobProperties = await blobRef.GetPropertiesAsync();
            
            Assert.Equal(contentType, blobProperties.Value.ContentType);
            Assert.Equal(contentDisposition, blobProperties.Value.ContentDisposition);
        }

        [Fact]
        public async void Test_Blob_Metadata_Updated_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var meta = new Dictionary<string, string>
            {
                { "key1", "val1" },
                { "key2", "val2" },
            };

            var containerRef = _client.GetBlobContainerClient(container);
            var blobRef = containerRef.GetBlobClient(blobName);

            await containerRef.CreateAsync();

            await blobRef.UploadAsync(data, new BlobUploadOptions
            {
                Metadata = meta
            });

            meta = new Dictionary<string, string>
            {
                { "key1", "somenewvalue" },
                { "key3", "val3" },
            };

            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties 
            { 
                Metadata = meta,
            });
            
            var blobProperties = await blobRef.GetPropertiesAsync();
            
            Assert.Equal(meta, blobProperties.Value.Metadata);
        }
    }
}