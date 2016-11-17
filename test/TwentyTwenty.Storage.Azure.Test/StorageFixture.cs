using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

namespace TwentyTwenty.Storage.Azure.Test
{
    public class StorageFixture : IDisposable
    {
        public const string ContainerPrefix = "storagetest-";
        public CloudStorageAccount _account;
        public CloudBlobClient _client;
        
        public StorageFixture()
        {   
            Config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."))
                .AddEnvironmentVariables()
                .AddUserSecrets()
                .Build();
            
            _client = CloudStorageAccount.Parse(Config["ConnectionString"]).CreateCloudBlobClient();
        }

        public IConfiguration Config { get; private set; }

        public void Dispose()
        {
            var list = new List<CloudBlobContainer>();
            BlobContinuationToken token = null;

            do
            {
                var results = _client.ListContainersSegmentedAsync(ContainerPrefix, token).Result;
                list.AddRange(results.Results);
            }
            while (token != null);

            foreach (var container in list)
            {
                container.DeleteAsync().Wait();
            }
        }
    }
}