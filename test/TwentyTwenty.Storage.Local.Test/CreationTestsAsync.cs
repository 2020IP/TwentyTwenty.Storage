using System.IO;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class CreationTestsAsync : BlobTestBase
    {
        public CreationTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Created()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data, closeStream: false);

            using (var file = File.OpenRead(Path.Combine(BasePath, container, blobName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        }

        [Fact]
        public async void Test_Blob_Created_RelativePath_BlobName()
        {
            var container = GetRandomContainerName();
            var blobName = "somePatch/test.txt";
            var data = GenerateRandomBlobStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data, closeStream: false);

            using (var file = File.OpenRead(Path.Combine(BasePath, container, blobName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        }
    }
}