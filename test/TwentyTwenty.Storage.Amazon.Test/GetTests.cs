using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class GetTests : BlobTestBase
    {
        public GetTests(StorageFixture fixture)
            : base(fixture) { }

        readonly HttpClient _httpClient = new HttpClient();

        [Fact]
        public async void Test_Get_Blob_Stream()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            await CreateNewObjectAsync(container, blobName, data);

            using (var blobStream = _provider.GetBlobStream(container, blobName))
            {
                var amzStream = new MemoryStream();
                blobStream.CopyTo(amzStream);

                Assert.True(StreamEquals(amzStream, stream));
            }
        }

        [Fact]
        public async void Test_Get_Blob_Sas_Url_Read()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            await CreateNewObjectAsync(container, blobName, data);

            var url = _provider.GetBlobSasUrl(container, blobName, expiry);

            Assert.NotEmpty(url);

            var downloadedData = await _httpClient.GetStreamAsync(url);
            Assert.True(StreamEquals(downloadedData, stream));
        }

        [Fact]
        public async void Test_Get_Blob_Sas_Url_Write()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);
            var dataCopy = new MemoryStream();
            data.CopyTo(dataCopy);
            data.Seek(0, SeekOrigin.Begin);

            var writeUrl = _provider.GetBlobSasUrl(container, blobName, expiry, access: BlobUrlAccess.Write);

            Assert.NotEmpty(writeUrl);

            var response = await _httpClient.PutAsync(writeUrl, new StreamContent(data));

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);

            var readUrl = _provider.GetBlobSasUrl(container, blobName, expiry);

            Assert.NotEmpty(readUrl);

            var downloadedData = await _httpClient.GetStreamAsync(readUrl);
            Assert.True(StreamEquals(downloadedData, dataCopy));
        }

        [Fact]
        public async void Test_Get_Blob_Sas_Url_Options()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var overrideContentType = "image/png";
            var overrideFilename = "testfilename.txt";
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            await CreateNewObjectAsync(container, blobName, data);

            var url = _provider.GetBlobSasUrl(container, blobName, expiry, true, overrideFilename, overrideContentType);

            Assert.NotEmpty(url);

            var response = await _httpClient.GetAsync(url);            
            Assert.Equal(response.Content.Headers.ContentType.MediaType, overrideContentType);
            Assert.Contains("attachment", response.Content.Headers.ContentDisposition.DispositionType);
            Assert.Contains(overrideFilename, response.Content.Headers.ContentDisposition.FileName);

            var contentStream = await response.Content.ReadAsStreamAsync();
            Assert.True(StreamEquals(contentStream, stream));            
        }

        [Fact]
        public async void Test_Get_Blob_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            await CreateNewObjectAsync(container, blobName, data, true);

            var url = _provider.GetBlobUrl(container, blobName);

            Assert.NotEmpty(url);

            var downloadedData = await _httpClient.GetStreamAsync(url);
            
            Assert.True(StreamEquals(downloadedData, stream));
        }

        [Fact]
        public async void Test_Get_Blob_Descriptor()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);
            var contentType = "image/png";

            await CreateNewObjectAsync(container, blobName, data, true, contentType);

            var descriptor = _provider.GetBlobDescriptor(container, blobName);

            Assert.Equal(descriptor.Container, container);
            Assert.NotEmpty(descriptor.ContentMD5);
            Assert.Equal(descriptor.ContentType, contentType);
            Assert.NotEmpty(descriptor.ETag);
            Assert.NotNull(descriptor.LastModified);
            Assert.Equal(descriptor.Length, datalength);
            Assert.Equal(descriptor.Name, blobName);
            Assert.Equal(descriptor.Security, BlobSecurity.Public);
        }

        [Fact]
        public async void Test_Get_Blob_List()
        {
            var container = GetRandomContainerName();

            await CreateNewObjectAsync(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/png");
            await CreateNewObjectAsync(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/jpg");
            await CreateNewObjectAsync(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "text/plain");

            foreach (var blob in _provider.ListBlobs(container))
            {
                var descriptor = _provider.GetBlobDescriptor(container, blob.Name);

                Assert.Equal(descriptor.Container, container);
                Assert.NotEmpty(descriptor.ContentMD5);
                Assert.Equal(descriptor.ContentType, blob.ContentType);
                Assert.NotEmpty(descriptor.ETag);
                Assert.NotNull(descriptor.LastModified);
                Assert.Equal(descriptor.Length, blob.Length);
                Assert.Equal(descriptor.Name, blob.Name);
                Assert.Equal(descriptor.Security, BlobSecurity.Private);
            }
        }
    }
}