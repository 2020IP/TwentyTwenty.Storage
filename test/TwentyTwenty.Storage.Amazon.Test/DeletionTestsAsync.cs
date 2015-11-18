using Amazon.S3.IO;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
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

            CreateNewObject(container, blobName, data);

            await _provider.DeleteContainerAsync(container);

            var objectsRequest = new ListObjectsRequest
            {
                BucketName = Bucket,
                Prefix = container,
                MaxKeys = 100000
            };

            var keys = new List<KeyVersion>();
            do
            {
                var objectsResponse = AsyncHelpers.RunSync(() => _client.ListObjectsAsync(objectsRequest));

                keys.AddRange(objectsResponse.S3Objects
                    .Select(x => new KeyVersion() { Key = x.Key, VersionId = null }));

                // If response is truncated, set the marker to get the next set of keys.
                if (objectsResponse.IsTruncated)
                {
                    objectsRequest.Marker = objectsResponse.NextMarker;
                }
                else
                {
                    objectsRequest = null;
                }
            } while (objectsRequest != null);

            Assert.Equal(keys.Count, 0);
        }

        [Fact]
        public async void Test_Blob_Deleted_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewObject(container, blobName, data);

            await _provider.DeleteBlobAsync(container, blobName);

            var info = new S3FileInfo(_client, Bucket, container + "/" + blobName);

            Assert.False(info.Exists);
        }
    }
}