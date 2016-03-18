using System;
using System.IO;
using System.Net;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class GetTests : BlobTestBase
    {
        public GetTests(StorageFixture fixture)
            : base(fixture) { }

        readonly WebClient _webClient = new WebClient();

        [Fact]
        public void Test_Get_Blob_Stream()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            CreateNewObject(container, blobName, data);

            using (var blobStream = _provider.GetBlobStream(container, blobName))
            {
                var amzStream = new MemoryStream();
                blobStream.CopyTo(amzStream);

                Assert.True(StreamEquals(amzStream, stream));
            }
        }

        [Fact]
        public void Test_Get_Blob_Sas_Url_Read()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            CreateNewObject(container, blobName, data);

            var url = _provider.GetBlobSasUrl(container, blobName, expiry);

            Assert.NotEmpty(url);

            var downloadedData = _webClient.DownloadData(url);
            Assert.True(StreamEquals(downloadedData.AsStream(), stream));
        }

        [Fact]
        public void Test_Get_Blob_Sas_Url_Write()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            var writeUrl = _provider.GetBlobSasUrl(container, blobName, expiry, access: BlobUrlAccess.Write);

            Assert.NotEmpty(writeUrl);

            var httpRequest = WebRequest.Create(writeUrl) as HttpWebRequest;
            httpRequest.Method = "PUT";
            using (var dataStream = httpRequest.GetRequestStream())
            {
                dataStream.Write(data.ToArray(), 0, (int)data.Length);
            }
            var response = httpRequest.GetResponse() as HttpWebResponse;
            Assert.Equal(response.StatusCode, HttpStatusCode.OK);

            var readUrl = _provider.GetBlobSasUrl(container, blobName, expiry);

            Assert.NotEmpty(readUrl);

            var downloadedData = _webClient.DownloadData(readUrl);
            Assert.True(StreamEquals(downloadedData.AsStream(), data));
        }

        [Fact]
        public void Test_Get_Blob_Sas_Url_Options()
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

            CreateNewObject(container, blobName, data);

            var url = _provider.GetBlobSasUrl(container, blobName, expiry, true, overrideFilename, overrideContentType);

            Assert.NotEmpty(url);

            var downloadedData = _webClient.DownloadData(url);
            Assert.True(StreamEquals(downloadedData.AsStream(), stream));
            Assert.Equal(_webClient.ResponseHeaders["Content-Type"], overrideContentType);
            Assert.Contains("attachment", _webClient.ResponseHeaders["Content-Disposition"]);
            Assert.Contains("filename=\"" + overrideFilename + "\"", _webClient.ResponseHeaders["Content-Disposition"]);
        }

        [Fact]
        public void Test_Get_Blob_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

            CreateNewObject(container, blobName, data, true);

            var url = _provider.GetBlobUrl(container, blobName);

            Assert.NotEmpty(url);

            var downloadedData = _webClient.DownloadData(url);
            
            Assert.True(StreamEquals(downloadedData.AsStream(), stream));
        }

        [Fact]
        public void Test_Get_Blob_Descriptor()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);
            var contentType = "image/png";

            CreateNewObject(container, blobName, data, true, contentType);

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
        public void Test_Get_Blob_List()
        {
            var container = GetRandomContainerName();

            CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/png");
            CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/jpg");
            CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "text/plain");

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