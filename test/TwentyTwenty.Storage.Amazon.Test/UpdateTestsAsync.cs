using Amazon.S3.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
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

            await CreateNewObjectAsync(container, blobName, data, false, contentType);

            await _provider.UpdateBlobPropertiesAsync(container, blobName, new BlobProperties
            {
                ContentType = newContentType,
                Security = BlobSecurity.Public
            });

            var objectMetaRequest = new GetObjectMetadataRequest()
            {
                BucketName = Bucket,
                Key = container + "/" + blobName
            };

            var props = await _client.GetObjectMetadataAsync(objectMetaRequest);

            Assert.Equal(props.Headers.ContentType, newContentType);

            var objectAclRequest = new GetACLRequest()
            {
                BucketName = Bucket,
                Key = container + "/" + blobName
            };

            var acl = await _client.GetACLAsync(objectAclRequest);

            var isPublic = acl.AccessControlList.Grants
                .Where(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers").Count() > 0;

            Assert.True(isPublic);
        }

        [Fact]
        public async void Test_Blob_ContentDisposition_Updated_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var filename = "testname.jpg";
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(container, blobName, data, false, contentType);

            var newProps = new BlobProperties
            {
                ContentType = newContentType,
                Security = BlobSecurity.Public,                
            }.WithContentDispositionFilename(filename);

            await _provider.UpdateBlobPropertiesAsync(container, blobName, newProps);

            var meta = await _client.GetObjectMetadataAsync(Bucket, container + "/" + blobName);

            Assert.Equal(newProps.ContentDisposition, meta.Headers.ContentDisposition);
        }

        [Fact]
        public async void Test_Blob_Meta_Updated()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var data = GenerateRandomBlobStream();
            var meta = new Dictionary<string, string>
            {
                //{ "FileName", "黑猫.jpeg" }, // Apparently amazon can't handle unicode in metadata.
                { "filename" , "test.jpg" }
            };

            await CreateNewObjectAsync(container, blobName, data, false, contentType);

            var newProps = new BlobProperties
            {
                ContentType = newContentType,
                Metadata = meta,
            };

            await _provider.UpdateBlobPropertiesAsync(container, blobName, newProps);

            var obj = await _client.GetObjectMetadataAsync(Bucket, container + "/" + blobName);

            Assert.Equal(meta, obj.Metadata.ToMetadata());
            Assert.Equal(newContentType, obj.Headers.ContentType);
        }
    }
}