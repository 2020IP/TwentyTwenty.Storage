using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class ExceptionTests : BlobTestBase
    {
        public ExceptionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Exception_BlobCreated_Auth()
        {
            var ex = Assert.Throws<StorageException>(() =>
            {
                var container = GetRandomContainerName();
                var blobName = GenerateRandomName();
                var data = GenerateRandomBlobStream();

                _exceptionProvider.SaveBlobStream(container, blobName, data);
            });

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public void Test_Exception_BlobDeleted_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            var ex = Assert.Throws<StorageException>(() =>
            {
                _exceptionProvider.DeleteBlob(container, blobName);
            });

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public void Test_Exception_ContainerDeleted_Auth()
        {
            var container = GetRandomContainerName();

            var ex = Assert.Throws<StorageException>(() =>
            {
                _exceptionProvider.DeleteContainer(container);
            });

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public void Test_Exception_GetBlobStream_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            var ex = Assert.Throws<StorageException>(() =>
            {
                _exceptionProvider.GetBlobStream(container, blobName);
            });

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }

        [Fact]
        public void Test_Exception_GetBlobDescriptor_Forbidden()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();

            var ex = Assert.Throws<StorageException>(() =>
            {
                _exceptionProvider.GetBlobDescriptor(container, blobName);
            });

            Assert.Equal((int)StorageErrorCode.InvalidAccess, ex.ErrorCode);
        }

        [Fact]
        public void Test_Exception_BlobPropertiesUpdated_Auth()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var newContentType = "image/png";

            var ex = Assert.Throws<StorageException>(() =>
            {
                _exceptionProvider.UpdateBlobProperties(container, blobName, new BlobProperties
                {
                    ContentType = newContentType,
                    Security = BlobSecurity.Public
                });
            });

            Assert.Equal((int)StorageErrorCode.InvalidCredentials, ex.ErrorCode);
        }
    }
}