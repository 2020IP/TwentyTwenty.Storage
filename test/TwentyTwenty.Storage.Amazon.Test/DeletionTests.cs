using Amazon.S3.IO;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class DeletionTests : BlobTestBase
    {
        public DeletionTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public void Test_Container_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewObject(container, blobName, data);

            _provider.DeleteContainer(container);

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
        public void Test_Blob_Deleted()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewObject(container, blobName, data);

            _provider.DeleteBlob(container, blobName);

            var info = new S3FileInfo(_client, Bucket, container + "/" + blobName);

            Assert.False(info.Exists);
        }
    }
}