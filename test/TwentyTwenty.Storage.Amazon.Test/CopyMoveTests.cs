using System.Threading.Tasks;
using Amazon.S3.Model;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class CopyMoveTests : BlobTestBase
    {
        public CopyMoveTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async Task Test_Blob_Moved()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            var amzObject = await _client.GetObjectAsync(Bucket, destContainer + "/" + sourceName, null);
            Assert.True(StreamEquals(amzObject.ResponseStream, data));

            // Make sure source no longer exists
            await Assert.ThrowsAsync<NoSuchKeyException>(() => _client.GetObjectAsync(Bucket, sourceContainer + "/" + sourceName, null));
        }

        [Fact]
        public async Task Test_Blob_Moved_And_Renamed()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            var amzObject = await _client.GetObjectAsync(Bucket, destContainer + "/" + destName, null);
            Assert.True(StreamEquals(amzObject.ResponseStream, data));

            // Make sure source no longer exists
            await Assert.ThrowsAsync<NoSuchKeyException>(() => _client.GetObjectAsync(Bucket, sourceContainer + "/" + sourceName, null));
        }

        [Fact]
        public async Task Test_Blob_Copied()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            var amzObject = await _client.GetObjectAsync(Bucket, destContainer + "/" + sourceName, null);
            Assert.True(StreamEquals(amzObject.ResponseStream, data));

            // Make sure source still exists
            amzObject = await _client.GetObjectAsync(Bucket, sourceContainer + "/" + sourceName, null);
            Assert.True(StreamEquals(amzObject.ResponseStream, data));
        }

        [Fact]
        public async Task Test_Blob_Copied_With_New_Name()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            var amzObject = await _client.GetObjectAsync(Bucket, destContainer + "/" + destName, null);
            Assert.True(StreamEquals(amzObject.ResponseStream, data));

            // Make sure source still exists
            amzObject = await _client.GetObjectAsync(Bucket, sourceContainer + "/" + sourceName, null);
            Assert.True(StreamEquals(amzObject.ResponseStream, data));
        }
    }
}