using System.IO;
using Xunit;
using FluentAssertions;
using FluentAssertions.Common;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class CreationTests : BlobTestBase
    {
        public CreationTests(StorageFixture fixture)
            : base(fixture)
        { }

        [Fact]
        public async void Test_Blob_Created()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;
            data.Position = 0;

            _provider.SaveBlobStream(container, blobName, stream);

            var blob = await _client.Objects.Get(Bucket, container + "/" + blobName).ExecuteAsync();

            blob.Should().NotBeNull();
            blob.MediaLink.Should().NotBeNullOrWhiteSpace();

            var googleStream = await _client.HttpClient.GetStreamAsync(blob.MediaLink);
            googleStream.Should().IsSameOrEqualTo(data);
        }

//        [Fact, Trait("Category", "Long")]
        public async void Test_Blob_Created_Resumable()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream(100000000);
            var stream = new MemoryStream();
            data.CopyTo(stream);
            stream.Position = 0;

            _provider.SaveBlobStream(container, blobName, stream);

            var blob = await _client.Objects.Get(Bucket, container + "/" + blobName).ExecuteAsync();

            blob.Should().NotBeNull();
            blob.MediaLink.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async void Test_Blob_Created_ContentType_Set()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var dataLength = 256;
            var data = GenerateRandomBlobStream(dataLength);

            _provider.SaveBlobStream(container, blobName, data, new BlobProperties { ContentType = contentType });

            var blob = await _client.Objects.Get(Bucket, container + "/" + blobName).ExecuteAsync();

            blob.Should().NotBeNull();
            blob.MediaLink.Should().NotBeNullOrWhiteSpace();
            blob.Size.IsSameOrEqualTo(dataLength);
        }
    }
}