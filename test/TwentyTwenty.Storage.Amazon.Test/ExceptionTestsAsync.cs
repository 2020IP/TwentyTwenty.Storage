using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class ExceptionTestsAsync : BlobTestBase
    {
        public ExceptionTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Exception_BlobCreatedAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _exceptionProvider.SaveBlobStreamAsync(container, blobName, data));

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Exception_BlobDeletedAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _exceptionProvider.DeleteBlobAsync(container, blobName));

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Exception_ContainerDeletedAsync_Auth()
        {
            var container = GetRandomContainerName();

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _exceptionProvider.DeleteContainerAsync(container));

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Exception_GetBlobStreamAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _exceptionProvider.GetBlobStreamAsync(container, blobName));

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptorAsync_Forbidden()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _exceptionProvider.GetBlobDescriptorAsync(container, blobName));

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Exception_BlobPropertiesUpdatedAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var newContentType = "image/png";

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _exceptionProvider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
                {
                    ContentType = newContentType,
                    Security = BlobSecurity.Public
                }));

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public async void Test_Exception_GetBlob_NotFound()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(container, blobName, data);

            var ex = await Assert.ThrowsAsync<StorageException>(()
                => _provider.GetBlobStreamAsync(container, GenerateRandomName()));

            Assert.Equal((int)StorageErrorCode.NotFound, ex.ErrorCode);
        }
    }
}