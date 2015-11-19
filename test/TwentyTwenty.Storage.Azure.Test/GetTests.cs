using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;
using System;
using System.Net.Http;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public sealed class GetTests : BlobTestBase
    {
        private HttpClient _httpClient = new HttpClient();

        public GetTests(StorageFixture fixture)
            : base(fixture)
        { }

        [Fact]
        public async void Test_Get_Blob_Descriptor()
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

            var descriptor = _provider.GetBlobDescriptor(container, blobName);

            await AssertBlobDescriptor(descriptor, blobRef);
        }

        //[Fact]
        //public async void Test_Get_Blob_Stream()
        //{
        //    var container = GetRandomContainerName();
        //    var blobName = GenerateRandomName();
        //    var data = GenerateRandomBlobStream();
        //    var containerRef = _client.GetContainerReference(container);

        //    await containerRef.CreateAsync();
        //    await containerRef.GetBlockBlobReference(blobName)
        //        .UploadFromStreamAsync(data);

        //    var blobStream = _provider.GetBlobStream(container, blobName);

        //    Assert.True(StreamEquals(blobStream, data));
        //}

        [Fact]
        public async void Test_Get_Blob_List()
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

            foreach (var blob in _provider.ListBlobs(container))
            {
                await AssertBlobDescriptor(blob, containerRef.GetBlockBlobReference(blob.Name));
            }
        }

        [Fact]
        public async void Test_Get_Blob_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            var url = _provider.GetBlobUrl(container, blobName);

            Assert.NotEmpty(url);

            var downloadedData = await _httpClient.GetStreamAsync(url);
            Assert.True(StreamEquals(downloadedData, data));
        }

        [Fact]
        public async void Test_Get_Blob_Sas_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            var url = _provider.GetBlobSasUrl(container, blobName, expiry);

            Assert.NotEmpty(url);
            Assert.Contains(expiry.ToString("s"), url);

            var downloadedData = await _httpClient.GetStreamAsync(url);
            Assert.True(StreamEquals(downloadedData, data));
        }

        [Fact]
        public async void Test_Get_Blob_Sas_Url_Options()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);
            var overrideContentType = "image/jpg";
            var overrideFilename = "test.jpg";

            await containerRef.CreateAsync();
            blobRef.Properties.ContentType = "image/png";
            await blobRef.UploadFromStreamAsync(data);

            var url = _provider.GetBlobSasUrl(container, blobName, expiry, true, overrideFilename, overrideContentType);

            Assert.NotEmpty(url);
            Assert.Contains(expiry.ToString("s"), url);

            var response = await _httpClient.GetAsync(url);

            Assert.True(StreamEquals(await response.Content.ReadAsStreamAsync(), data));
            Assert.Equal(overrideContentType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
            Assert.Equal("\"" + overrideFilename + "\"", response.Content.Headers.ContentDisposition.FileName);
        }
    }
}