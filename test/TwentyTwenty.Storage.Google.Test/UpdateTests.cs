using System;
using Google.Apis.Storage.v1;
using System.Linq;
using Xunit;
using Google.Apis.Storage.v1.Data;
using Newtonsoft.Json.Linq;
using Google.Cloud.Storage.V1;
using System.Collections.Generic;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class UpdateTests : BlobTestBase
    {
        public UpdateTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Properties_Updated_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var newContentDisposition = "attachment; filename=\"filename.jpg\"";
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), contentType, data);

            var blob = await _client.GetObjectAsync(Bucket, GetObjectName(container, blobName), new GetObjectOptions { Projection = Projection.Full });
            var timestamp = blob.Updated;
            Assert.Equal(contentType, blob.ContentType);
            Assert.Null(blob.ContentDisposition);
            Assert.Null(blob.Metadata);
            Assert.DoesNotContain(blob.Acl, o => o.Entity == "allUsers");
            
            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
            {
                ContentType = newContentType,
                ContentDisposition = newContentDisposition,
                Security = BlobSecurity.Public,
                Metadata = new Dictionary<string, string>
                {
                    { "test", "a" }
                },
            });

            blob = await _client.GetObjectAsync(Bucket, GetObjectName(container, blobName), new GetObjectOptions { Projection = Projection.Full });

            Assert.Equal(newContentType, blob.ContentType);
            Assert.Equal(newContentDisposition, blob.ContentDisposition);
            Assert.NotEqual(timestamp, blob.Updated);
            Assert.Contains(blob.Acl, o => o.Entity == "allUsers" && o.Role == "READER");
            Assert.NotNull(blob.Metadata);
            Assert.Contains(blob.Metadata, m => m.Key == "test" && m.Value == "a");

            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
            {
                ContentType = newContentType,
                Security = BlobSecurity.Private
            });

            blob = await _client.GetObjectAsync(Bucket, GetObjectName(container, blobName), new GetObjectOptions { Projection = Projection.Full });

            Assert.Equal(newContentType, blob.ContentType);
            Assert.NotEqual(timestamp, blob.Updated);
            Assert.DoesNotContain(blob.Acl, o => o.Entity == "allUsers");
        }
    }
}