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
        public async void Test_Blob_Created_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _provider.SaveBlobStreamAsync(container, blobName, data);

            using (var file = File.OpenRead($"{BasePath}\\{container}\\{blobName}"))
            {
                Assert.True(StreamEquals(data, file));
            }
        }
    }
}