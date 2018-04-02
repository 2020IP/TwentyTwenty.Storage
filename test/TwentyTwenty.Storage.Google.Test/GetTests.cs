using System;
using System.IO;
using System.Net;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Xunit;
using GoogleCredential = Google.Apis.Auth.OAuth2.GoogleCredential;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class GetTests : BlobTestBase
    {
        public GetTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            using (var blobStream = await _provider.GetBlobStreamAsync(container, blobName))
            {
                Assert.True(StreamEquals(blobStream, data));
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

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), contentType, data);

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

            Assert.Equal(descriptor.Container, container);
            Assert.NotEmpty(descriptor.ContentMD5);
            Assert.Equal(descriptor.ContentType, contentType);
            Assert.NotEmpty(descriptor.ETag);
            Assert.NotNull(descriptor.LastModified);
            Assert.Equal(datalength, descriptor.Length);
            Assert.Equal(blobName, descriptor.Name);
            Assert.Equal(BlobSecurity.Private, descriptor.Security);
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, GenerateRandomName()), 
                "image/png", GenerateRandomBlobStream());
            await _client.UploadObjectAsync(Bucket, GetObjectName(container, GenerateRandomName()), 
                "image/jpg", GenerateRandomBlobStream());
            await _client.UploadObjectAsync(Bucket, GetObjectName(container, GenerateRandomName()), 
                "text/plain", GenerateRandomBlobStream());

            var list = await _provider.ListBlobsAsync(container);

            Assert.Equal(3, list.Count);

            foreach (var blob in list)
            {
                var descriptor = await _provider.GetBlobDescriptorAsync(container, blob.Name);

                Assert.Equal(descriptor.Container, container);
                Assert.NotEmpty(descriptor.ContentMD5);
                Assert.Equal(descriptor.ContentType, blob.ContentType);
                Assert.NotEmpty(descriptor.ETag);
                Assert.NotNull(descriptor.LastModified);
                Assert.Equal(descriptor.Length, blob.Length);
                Assert.Equal(descriptor.Name, blob.Name);
                Assert.Equal(BlobSecurity.Private, descriptor.Security);
            }
        }
    }
}