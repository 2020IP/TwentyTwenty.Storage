using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Google;
using TwentyTwenty.Storage.Google.Test;
using Xunit;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class DeletionTestsAsync : BlobTestBase
    {
        public DeletionTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Container_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data);

            await _provider.DeleteContainerAsync(container);

            var req = _client.Objects.List(Bucket);
            req.Prefix = container;
            var result = await req.ExecuteAsync();

            result.Items.Should().BeNullOrEmpty();
        }

        [Fact]
        public async void Test_Blob_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var googleObjectName = container + "/" + blobName;
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data);

            await _provider.DeleteBlobAsync(container, blobName);

            var ex = await Assert.ThrowsAsync<GoogleApiException>(() => _client.Objects.Get(Bucket, googleObjectName).ExecuteAsync());

            ex.Should().NotBeNull();
            ex.Message.Should().Contain("404");
        }
    }
}