using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture)
        { }

        [Fact]
        public void Test_Exception_DeleteBlob_Auth()
        {
            TestProviderAuth(provider => provider.DeleteBlob("asdf", "asdf"));
        }

        [Fact]
        public void Test_Exception_DeleteContainer_Auth()
        {
            TestProviderAuth(provider => provider.DeleteContainer("asdf"));
        }

        [Fact]
        public async void Test_Exception_UpdateBlobProperties_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            TestProviderAuth(provider => provider.UpdateBlobProperties(container, blobName, new BlobProperties()));
        }

        [Fact]
        public void Test_Exception_SaveBlobStream_Auth()
        {
            TestProviderAuth(provider => provider.SaveBlobStream("asdf", "asdf", new MemoryStream()));
        }

        [Fact]
        public async void Test_Exception_GetBlobStream_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            TestProviderAuth(provider => provider.GetBlobStream(container, blobName));
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptor_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);


            TestProviderAuth(provider => provider.GetBlobDescriptor(container, blobName));
        }

        [Fact]
        public async void Test_Exception_ListBlob_Auth()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            TestProviderAuth(provider => provider.ListBlobs(container));
        }

        [Fact]
        public void Test_Exception_ListBlob_ContainerName()
        {
            var exception = Assert.Throws<StorageException>(() =>
            {
                _provider.ListBlobs("asdf");
            });
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidName);
        }

        [Fact]
        public async void Test_Exception_GetBlobStream_BlobName()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            var exception = Assert.Throws<StorageException>(() =>
            {
                _provider.GetBlobStream(container, "asdf");
            });
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidName);
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptor_BlobName()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            var exception = Assert.Throws<StorageException>(() =>
            {
                _provider.GetBlobDescriptor(container, "asdf");
            });
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidName);
        }

        [Fact]
        public async void Test_Exception_UpdateBlobProperties_BlobName()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            var exception = Assert.Throws<StorageException>(() => _provider.UpdateBlobProperties(container, "asdf", new BlobProperties()));
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidName);
        }
    }
}