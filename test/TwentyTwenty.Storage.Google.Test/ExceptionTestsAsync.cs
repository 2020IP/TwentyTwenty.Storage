using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
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

            await TestProviderAuthAsync(p => p.SaveBlobStreamAsync(container, blobName, data));
        }

        [Fact]
        public async void Test_Exception_BlobDeletedAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            await TestProviderAuthAsync(p => p.DeleteBlobAsync(container, blobName));
        }

        [Fact]
        public async void Test_Exception_ContainerDeletedAsync_Auth()
        {
            var container = GetRandomContainerName();

            await TestProviderAuthAsync(p => p.DeleteContainerAsync(container));
        }

        [Fact]
        public async void Test_Exception_GetBlobStreamAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);
            await TestProviderAuthAsync(p => p.GetBlobStreamAsync(container, blobName));
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptorAsync_Forbidden()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);
            await TestProviderAuthAsync(p => p.GetBlobDescriptorAsync(container, blobName));
        }

        [Fact]
        public async void Test_Exception_BlobPropertiesUpdatedAsync_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);
            await TestProviderAuthAsync(p => p.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
            {
                ContentType = newContentType,
                Security = BlobSecurity.Public
            }));
        }
    }
}