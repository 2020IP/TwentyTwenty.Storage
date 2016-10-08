using Amazon.S3.Model;
using System.Linq;
using Xunit;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [Trait("Category", "Amazon")]
    public sealed class UpdateTests : BlobTestBase
    {
        public UpdateTests(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Blob_Properties_Updated()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var newContentType = "image/png";
            var data = GenerateRandomBlobStream();

            await CreateNewObjectAsync(container, blobName, data, false, contentType);

            _provider.UpdateBlobProperties(container, blobName, new BlobProperties
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
        public async void Test_Blob_ContentDisposition_Updated()
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

            _provider.UpdateBlobProperties(container, blobName, newProps);

            var meta = await _client.GetObjectMetadataAsync(Bucket, container + "/" + blobName);

            Assert.Equal(newProps.ContentDisposition, meta.Headers.ContentDisposition);
        }
    }
}