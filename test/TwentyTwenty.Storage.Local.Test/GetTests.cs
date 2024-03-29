﻿using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class GetTests : BlobTestBase
    {
        public GetTests()
            : base() { }

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            using var blobStream = await _provider.GetBlobStreamAsync(container, blobName);
            Assert.True(StreamEquals(data, blobStream));
        }

        [Fact]
        public async void Test_Get_Blob_Descriptor_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName() + ".json";
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);

            CreateNewFile(container, blobName, data);

            var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

            Assert.Equal(descriptor.Container, container);
            Assert.Equal("application/json", descriptor.ContentType);
            Assert.NotNull(descriptor.LastModified);
            Assert.Equal(descriptor.Length, datalength);
            Assert.Equal(descriptor.Name, blobName);
            Assert.Equal(BlobSecurity.Private, descriptor.Security);
        }

        [Fact]
        public async void Test_Does_Blob_Exist_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName() + ".json";
            var datalength = 256;
            var data = GenerateRandomBlobStream(datalength);

            CreateNewFile(container, blobName, data);

            Assert.True(await _provider.DoesBlobExistAsync(container, blobName));
            Assert.False(await _provider.DoesBlobExistAsync(container, "fake.json"));
        }

        [Fact]
        public async void Test_Get_Blob_List_Async()
        {
            var container = GetRandomContainerName();

            CreateNewFile(container, GenerateRandomName() + ".json", GenerateRandomBlobStream());
            CreateNewFile(container, GenerateRandomName(), GenerateRandomBlobStream());
            CreateNewFile(container, GenerateRandomName(), GenerateRandomBlobStream());

            foreach (var blob in await _provider.ListBlobsAsync(container))
            {
                var descriptor = _provider.GetBlobDescriptor(container, blob.Name);

                Assert.Equal(descriptor.Container, container);
                Assert.Equal(descriptor.ContentType, blob.ContentType);
                Assert.NotNull(descriptor.LastModified);
                Assert.Equal(descriptor.Length, blob.Length);
                Assert.Equal(descriptor.Name, blob.Name);
                Assert.Equal(BlobSecurity.Private, descriptor.Security);
            }
        }

        // Test for #13
        [Fact]
        public async void Test_Get_Deep_Blob_List()
        {
            var container = GetRandomContainerName();

            CreateNewFile(container, GenerateRandomName() + ".json", GenerateRandomBlobStream());
            CreateNewFile(container, GenerateRandomName() + "/myfile.txt", GenerateRandomBlobStream());
            CreateNewFile(container, GenerateRandomName(), GenerateRandomBlobStream());

            var list = await _provider.ListBlobsAsync(container);
            Assert.Equal(3, list.Count);
        }
        
        [Theory]
        [InlineData("test", false)]
        [InlineData("dir/test", false)]
        [InlineData("dir/..//test", false)]
        [InlineData("dir\\..\\test", false)]
        [InlineData("../test", true)]
        [InlineData("..\\test", false)]
        [InlineData("...\\.\\test", false)]
        [InlineData("dir\\...\\.\\test", false)]
        public async void Test_Path_Traversal_Check(string blobName, bool shouldBeThrowing)
        {
            async Task TestCode()
            {
                var container = GetRandomContainerName();
                var data = GenerateRandomBlobStream();
                await _provider.SaveBlobStreamAsync(container, blobName, data, BlobProperties.Empty, false);

                var containingDirectory = Path.Combine(BasePath, container);
                var realFullPath = Path.GetFullPath(Path.Combine(containingDirectory, blobName));

                Assert.StartsWith(containingDirectory, realFullPath);
                using var file = File.OpenRead(Path.Combine(BasePath, container, blobName));
                Assert.True(StreamEquals(data, file));
            }
            if (shouldBeThrowing)
            {
                await Assert.ThrowsAsync<StorageException>(TestCode);
            }
            else
            {
                await TestCode();
            }
        }
        
    }
}