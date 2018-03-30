using System;
using System.IO;
using System.Net;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Xunit;
using GoogleCredential = Google.Apis.Auth.OAuth2.GoogleCredential;

namespace TwentyTwenty.Storage.Google.Test
{
    public class TestTestsTest
    {
        // [Fact]
        public void Test_Get_Blob_Stream2_Async()
        {
            try
            {
                var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."))
                .AddEnvironmentVariables()
                .AddUserSecrets<StorageFixture>()
                .Build();

                var credBytes = Convert.FromBase64String(config["GoogleCredJsonBase64"]);
                var credJson = System.Text.Encoding.UTF8.GetString(credBytes);
                var cred = GoogleCredential.FromJson(credJson);

                var client = StorageClient.Create(cred);

                var d = System.Text.Encoding.UTF8.GetBytes("Quack Data");
                var s = new MemoryStream(d);

                client.UploadObject("2020-storage-test1", "muh-object2", null, s);
            }
            catch (System.Exception e)
            {
                
                Console.WriteLine(e);
            }
        }
    }


    [Trait("Category", "Google")]
    public sealed class GetTestsAsync : BlobTestBase
    {
        public GetTestsAsync(StorageFixture fixture)
            : base(fixture) { }

        [Fact]
        public async void Test_Get_Blob_Stream_Async()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            await _client.UploadObjectAsync(Bucket, GetObjectName(container, blobName), null, data);

            using (var blobStream = await _provider.GetBlobStreamAsync(container, blobName))
            {
                Assert.True(StreamEquals(blobStream, data));
            }
        }

        // [Fact]
        // public async void Test_Get_Blob_Descriptor_Async()
        // {
        //     var container = GetRandomContainerName();
        //     var blobName = GenerateRandomName();
        //     var datalength = 256;
        //     var data = GenerateRandomBlobStream(datalength);
        //     var contentType = "image/png";

        //     await CreateNewObject(container, blobName, data, false, contentType);

        //     var descriptor = await _provider.GetBlobDescriptorAsync(container, blobName);

        //     Assert.Equal(descriptor.Container, container);
        //     Assert.NotEmpty(descriptor.ContentMD5);
        //     Assert.Equal(descriptor.ContentType, contentType);
        //     Assert.NotEmpty(descriptor.ETag);
        //     Assert.NotNull(descriptor.LastModified);
        //     Assert.Equal(descriptor.Length, datalength);
        //     Assert.Equal(descriptor.Name, blobName);
        //     Assert.Equal(descriptor.Security, BlobSecurity.Private);
        // }

        // [Fact]
        // public async void Test_Get_Blob_List_Async()
        // {
        //     var container = GetRandomContainerName();

        //     await CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/png");
        //     await CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "image/jpg");
        //     await CreateNewObject(container, GenerateRandomName(), GenerateRandomBlobStream(), false, "text/plain");

        //     foreach (var blob in await _provider.ListBlobsAsync(container))
        //     {
        //         var descriptor = await _provider.GetBlobDescriptorAsync(container, blob.Name);

        //         Assert.Equal(descriptor.Container, container);
        //         Assert.NotEmpty(descriptor.ContentMD5);
        //         Assert.Equal(descriptor.ContentType, blob.ContentType);
        //         Assert.NotEmpty(descriptor.ETag);
        //         Assert.NotNull(descriptor.LastModified);
        //         Assert.Equal(descriptor.Length, blob.Length);
        //         Assert.Equal(descriptor.Name, blob.Name);
        //         Assert.Equal(descriptor.Security, BlobSecurity.Private);
        //     }
        // }
    }
}