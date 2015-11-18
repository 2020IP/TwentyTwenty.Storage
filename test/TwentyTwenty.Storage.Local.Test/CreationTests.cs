using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class CreationTests : BlobTestBase
    {
        public CreationTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Blob_Created()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            _provider.SaveBlobStream(container, blobName, data);

            Assert.True(StreamEquals(data, File.OpenRead($"{BasePath}\\{container}\\{blobName}")));
        }
    }
}