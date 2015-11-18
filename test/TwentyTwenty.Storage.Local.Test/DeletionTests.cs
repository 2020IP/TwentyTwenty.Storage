using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class DeletionTests : BlobTestBase
    {
        public DeletionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Container_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            _provider.DeleteContainer(container);

            Assert.False(Directory.Exists($"{BasePath}\\{container}"));
        }

        [Fact]
        public void Test_Blob_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(container, blobName, data);

            _provider.DeleteBlob(container, blobName);

            Assert.False(File.Exists($"{BasePath}\\{container}\\{blobName}"));
        }
    }
}