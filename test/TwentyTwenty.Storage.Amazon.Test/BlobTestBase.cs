using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Threading.Tasks;

namespace TwentyTwenty.Storage.Amazon.Test
{
    [CollectionDefinition("BlobTestBase")]
    public class BaseCollection : ICollectionFixture<StorageFixture>
    {
    }

    [Collection("BlobTestBase")]
    public abstract class BlobTestBase
    {
        Random _rand = new Random();
        protected AmazonS3Client _client;
        protected IStorageProvider _provider;
        protected IStorageProvider _exceptionProvider;
        protected string Bucket;
        protected string ContainerPrefix;

        public BlobTestBase(StorageFixture fixture)
        {
            Bucket = fixture.Config["Bucket"];
            ContainerPrefix = StorageFixture.ContainerPrefix;
            _client = fixture._client;

            _provider = new AmazonStorageProvider(new AmazonProviderOptions
            {
                Bucket = fixture.Config["Bucket"],
                PublicKey = fixture.Config["PublicKey"],
                SecretKey = fixture.Config["PrivateKey"]
            });

            // Dumby provider to test exception throwing
            _exceptionProvider = new AmazonStorageProvider(new AmazonProviderOptions
            {
                Bucket = fixture.Config["Bucket"],
                PublicKey = "aaa",
                SecretKey = "aaa"
            });
        }

        protected byte[] GenerateRandomBlob(int length = 256)
        {
            var buffer = new Byte[length];
            _rand.NextBytes(buffer);
            return buffer;
        }

        protected MemoryStream GenerateRandomBlobStream(int length = 256)
        {
            return new MemoryStream(GenerateRandomBlob(length));
        }

        protected string GetRandomContainerName()
        {
            return StorageFixture.ContainerPrefix + Guid.NewGuid().ToString("N");
        }

        protected string GenerateRandomName()
        {
            return Guid.NewGuid().ToString("N");
        }

        protected async Task CreateNewObjectAsync(string container, string blobName, Stream data, bool isPublic = false, string contentType = null)
        {
            var putRequest = new PutObjectRequest()
            {
                BucketName = Bucket,
                Key = container + "/" + blobName,
                InputStream = data,
                CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private,
                ContentType = contentType
            };

            await _client.PutObjectAsync(putRequest);
        }

        protected bool StreamEquals(Stream stream1, Stream stream2)
        {
            if (stream1.CanSeek)
            {
                stream1.Seek(0, SeekOrigin.Begin);
            }
            if (stream2.CanSeek)
            {
                stream2.Seek(0, SeekOrigin.Begin);
            }

            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize]; //buffer size
            byte[] buffer2 = new byte[bufferSize];
            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                // You might replace the following with an efficient "memcmp"
                if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
                    return false;
            }
        }
    }
}