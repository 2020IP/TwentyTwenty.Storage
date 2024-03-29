﻿using System;
using Google.Cloud.Storage.V1;
using Xunit;

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

            using var blobStream = await _provider.GetBlobStreamAsync(container, blobName);
            Assert.True(StreamEquals(blobStream, data));
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
        public async void Test_Does_Blob_Exist_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);
            var contentType = "image/png";

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), contentType, data);

            Assert.True(await _provider.DoesBlobExistAsync(container, blobName));
            Assert.False(await _provider.DoesBlobExistAsync(container, "fake"));
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, GenerateRandomName()),
                "image/png", GenerateRandomBlobStream());
            await _client.UploadObjectAsync(Bucket, GetObjectName(container, GenerateRandomName()),
                "image/jpg", GenerateRandomBlobStream(), new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead });
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
                Assert.Equal(descriptor.Security, blob.Security);
            }
        }

        [Fact]
        public async void Test_Get_Blob_Security_Public()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data,
                new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead });

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);
            Assert.Equal(BlobSecurity.Public, descriptor.Security);
        }

        [Fact]
        public async void Test_Get_Blob_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            var url = _provider.GetBlobUrl(container, blobName);
            Assert.NotEmpty(url);

            Console.WriteLine("URL: " + url);
        }

        [Fact]
        public async void Test_Get_Blob_Sas_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            var url = _provider.GetBlobSasUrl(container, blobName, DateTimeOffset.Now.AddHours(1), contentType: "text/plain");
            Assert.NotEmpty(url);

            Console.WriteLine("SAS URL: " + url);
        }
    }
}