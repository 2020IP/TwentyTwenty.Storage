using System;
using Google.Apis.Storage.v1;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using FluentAssertions.Common;
using Google;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class DeletionTests : BlobTestBase
    {
        public DeletionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Container_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data);

            _provider.DeleteContainer(container);

            var req = _client.Objects.List(Bucket);
            req.Prefix = container;
            var result = await req.ExecuteAsync();

            result.Items.Should().BeNullOrEmpty();
        }

        [Fact]
        public async void Test_Blob_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var googleObjectName = container + "/" + blobName;
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data);

            _provider.DeleteBlob(container, blobName);

            var ex = await Assert.ThrowsAsync<GoogleApiException>(() => _client.Objects.Get(Bucket, googleObjectName).ExecuteAsync());

            ex.Should().NotBeNull();
            ex.Message.Should().Contain("404");
        }
    }
}