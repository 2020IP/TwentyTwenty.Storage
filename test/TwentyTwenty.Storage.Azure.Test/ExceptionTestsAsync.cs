using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public class ExceptionTestsAsync : BlobTestBase
    {
        public ExceptionTestsAsync(StorageFixture fixture)
            : base(fixture)
        { }

        [Fact]
        public async void Test_Exception_DeleteBlobAsync_Auth()
        {
            await TestProviderAuthAsync(provider => provider.DeleteBlobAsync("asdf", "asdf"));
        }

        [Fact]
        public async void Test_Exception_DeleteContainerAsync_Auth()
        {
            await TestProviderAuthAsync(provider => provider.DeleteContainerAsync("asdf"));
        }

        [Fact]
        public async void Test_Exception_UpdateBlobPropertiesAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            await TestProviderAuthAsync(provider => provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties()));
        }

        [Fact]
        public async void Test_Exception_SaveBlobStreamAsync_Auth()
        {
            await TestProviderAuthAsync(provider => provider.SaveBlobStreamAsync("asdf", "asdf", new MemoryStream()));
        }

        [Fact]
        public async void Test_Exception_GetBlobStreamAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            await TestProviderAuthAsync(provider => provider.GetBlobStreamAsync(container, blobName));
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptorAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();
            await containerRef.GetBlockBlobReference(blobName)
                .UploadFromStreamAsync(data);

            await TestProviderAuthAsync(provider => provider.GetBlobDescriptorAsync(container, blobName));
        }

        [Fact]
        public async void Test_Exception_ListBlobAsync_Auth()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            await TestProviderAuthAsync(provider => provider.ListBlobsAsync(container));
        }

        [Fact]
        public async void Test_Exception_ListBlobAsync_ContainerName()
        {
            var exception = await Assert.ThrowsAsync<StorageException>(() => _provider.ListBlobsAsync("asdf"));
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidContainerName);
        }

        [Fact]
        public async void Test_Exception_GetBlobStreamAsync_BlobName()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            var exception = await Assert.ThrowsAsync<StorageException>(() => _provider.GetBlobStreamAsync(container, "asdf"));
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidBlobName);
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptorAsync_BlobName()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            var exception = await Assert.ThrowsAsync<StorageException>(() => _provider.GetBlobDescriptorAsync(container, "asdf"));
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidBlobName);
        }

        [Fact]
        public async void Test_Exception_UpdateBlobPropertiesAsync_BlobName()
        {
            var container = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(container);

            await containerRef.CreateAsync();

            var exception = await Assert.ThrowsAsync<StorageException>(() => _provider.UpdateBlobPropertiesAsync(container, "asdf", new BlobProperties()));
            Assert.Equal(exception.ErrorCode, (int)StorageErrorCode.InvalidBlobName);
        }
    }
}