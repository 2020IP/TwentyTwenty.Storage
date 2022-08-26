using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.IO;

namespace TwentyTwenty.Storage.Amazon.Test
{
    public sealed class StorageFixture : IDisposable
    {
        public const string ContainerPrefix = "storagetest-";
        public readonly AmazonS3Client _client;

        public StorageFixture()
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."))
                .AddEnvironmentVariables()
                .AddUserSecrets<StorageFixture>()
                .Build();

            var S3Config = new AmazonS3Config
            {
                ServiceURL = "https://s3.amazonaws.com"
            };
            
            _client = new AmazonS3Client(Config["PublicKey"], Config["PrivateKey"], S3Config);
        }

        public IConfiguration Config { get; private set; }

        public void Dispose()
        {
            var objectsRequest = new ListObjectsRequest
            {
                BucketName = Config["Bucket"],
                Prefix = ContainerPrefix,
                MaxKeys = 1000
            };

            var keys = new List<KeyVersion>();
            do
            {
                var objectsResponse = _client.ListObjectsAsync(objectsRequest).Result;

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

            if (keys.Count > 0)
            {
                var objectsDeleteRequest = new DeleteObjectsRequest()
                {
                    BucketName = Config["Bucket"],
                    Objects = keys
                };

                _client.DeleteObjectsAsync(objectsDeleteRequest).Wait();
            }
        }
    }
}
