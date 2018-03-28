using System;
using Google.Apis.Storage.v1;
using System.Linq;
using Xunit;
using FluentAssertions;
using Google.Apis.Storage.v1.Data;
using Newtonsoft.Json.Linq;

namespace TwentyTwenty.Storage.Google.Test
{
    [Trait("Category", "Google")]
    public sealed class UpdateTestsAsync : BlobTestBase
    {
        public UpdateTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Properties_Updated_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var data = GenerateRandomBlobStream();

            await CreateNewObject(container, blobName, data, false, contentType);

            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
            {
                ContentType = newContentType,
                Security = BlobSecurity.Public
            });

            var blob = await _client.Objects.Get(Bucket, container + "/" + blobName).ExecuteAsync();
	        blob.Acl = (await _client.ObjectAccessControls.List(Bucket, blob.Name)
			        .ExecuteAsync())
					.Items;

            blob.Should().NotBeNull();
            blob.ContentType.Should().Be(newContentType);

            blob.Acl.Should().NotBeNullOrEmpty();
            blob.Acl.Should().ContainSingle(o => o.Entity == "allUsers" && o.Role == "READER");
        }
    }
}