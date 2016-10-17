using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
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
            var containerRef = _client.GetContainerReference(containerName);

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Off, null, null);

            await _provider.SaveBlobStreamAsync(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Public });
            
            Assert.True(await containerRef.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Blob, (await containerRef.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Container_Permissions_Elevated_On_Update_Async()
        {
            var containerName = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var containerRef = _client.GetContainerReference(containerName);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            var data = GenerateRandomBlobStream();

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Off, null, null);

            await blobRef.UploadFromStreamAsync(data);
            await _provider.UpdateBlobPropertiesAsync(containerName, blobName, new BlobProperties { Security = BlobSecurity.Public });

            Assert.Equal(BlobContainerPublicAccessType.Blob, (await containerRef.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Blob_Properties_Updated_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var contentDisposition = "attachment; filename=\"muhFile.jpg\"";
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);

            await containerRef.CreateAsync();
            blobRef.Properties.ContentType = "image/png";

            await blobRef.UploadFromStreamAsync(data);
            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties 
            { 
                ContentType = contentType,
                ContentDisposition = contentDisposition,
            });
            
            await blobRef.FetchAttributesAsync();
            
            Assert.Equal(contentType, blobRef.Properties.ContentType);
            Assert.Equal(contentDisposition, blobRef.Properties.ContentDisposition);
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

            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);

            await containerRef.CreateAsync();

            foreach (var kvp in meta)
            {            
                blobRef.Metadata.Add(kvp.Key, kvp.Value);
            }

            await blobRef.UploadFromStreamAsync(data);

            meta = new Dictionary<string, string>
            {
                { "key1", "somenewvalue" },
                { "key3", "val3" },
            };

            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties 
            { 
                Metadata = meta,
            });
            
            await blobRef.FetchAttributesAsync();
            
            Assert.Equal(meta, blobRef.Metadata);
        }
    }
}