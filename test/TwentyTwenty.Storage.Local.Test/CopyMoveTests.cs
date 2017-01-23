using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class CopyMoveTests : BlobTestBase
    {
        public CopyMoveTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Moved()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
        }

        [Fact]
        public async void Test_Blob_Moved_And_Renamed()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
        }

        [Fact]
        public async void Test_Blob_Copied()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer);

            data.ToString();
            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        }

        [Fact]
        public async void Test_Blob_Copied_With_New_Name()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        }
    }
}