using System;
using System.IO;
using System.Net;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
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

            CreateNewFile(container, blobName, data);

            using (var blobStream = _provider.GetBlobStream(container, blobName))
            {
                Assert.True(StreamEquals(data, blobStream));
            }
        }

        [Fact]
        public void Test_Get_Blob_Sas_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            var url = _provider.GetBlobSasUrl(container, blobName, new DateTimeOffset());

            Assert.NotEmpty(url);
            Assert.True(StreamEquals(File.OpenRead(url), data));
        }

        [Fact]
        public void Test_Get_Blob_Url()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            var url = _provider.GetBlobSasUrl(container, blobName, new DateTimeOffset());

            Assert.NotEmpty(url);
            Assert.True(StreamEquals(File.OpenRead(url), data));
        }

        [Fact]
        public void Test_Get_Blob_Descriptor()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName() + ".json";
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);

            CreateNewFile(container, blobName, data);

            var descriptor = _provider.GetBlobDescriptor(container, blobName);

            Assert.Equal(descriptor.Container, container);
            Assert.Equal(descriptor.ContentType, "application/json");
            Assert.NotNull(descriptor.LastModified);
            Assert.Equal(descriptor.Length, datalength);
            Assert.Equal(descriptor.Name, blobName);
            Assert.Equal(descriptor.Security, BlobSecurity.Private);
        }

        [Fact]
        public void Test_Get_Blob_List()
        {
            var container = GetRandomContainerName();

            CreateNewFile(container, GenerateRandomName() + ".json", GenerateRandomBlobStream());
            CreateNewFile(container, GenerateRandomName(), GenerateRandomBlobStream());
            CreateNewFile(container, GenerateRandomName(), GenerateRandomBlobStream());

            foreach (var blob in _provider.ListBlobs(container))
            {
                var descriptor = _provider.GetBlobDescriptor(container, blob.Name);

                Assert.Equal(descriptor.Container, container);
                Assert.Equal(descriptor.ContentType, blob.ContentType);
                Assert.NotNull(descriptor.LastModified);
                Assert.Equal(descriptor.Length, blob.Length);
                Assert.Equal(descriptor.Name, blob.Name);
                Assert.Equal(descriptor.Security, BlobSecurity.Private);
            }
        }
    }
}