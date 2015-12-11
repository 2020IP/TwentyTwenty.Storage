using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Exception_BlobCreated_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            TestProviderAuth(p => p.SaveBlobStream(container, blobName, data));
        }

        [Fact]
        public void Test_Exception_BlobDeleted_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            TestProviderAuth(p => p.DeleteBlob(container, blobName));
        }

        [Fact]
        public void Test_Exception_ContainerDeleted_Auth()
        {
            var container = GetRandomContainerName();

            TestProviderAuth(p => p.DeleteContainer(container));
        }

        [Fact]
        public async void Test_Exception_GetBlobStream_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);
            TestProviderAuth(p => p.GetBlobStream(container, blobName));
        }

        [Fact]
        public async void Test_Exception_GetBlobDescriptor_Forbidden()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);
            TestProviderAuth(p => p.GetBlobDescriptor(container, blobName));
        }

        [Fact]
        public async void Test_Exception_BlobPropertiesUpdated_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);
            TestProviderAuth(p => p.UpdateBlobProperties(container, blobName, new BlobProperties
            {
                ContentType = newContentType,
                Security = BlobSecurity.Public
            }));
        }
    }
}