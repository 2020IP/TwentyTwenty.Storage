using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace TwentyTwenty.Storage.Azure.Test
{
    public class StorageFixture : IDisposable
    {
        public const string ContainerPrefix = "storagetest-";
        public BlobServiceClient _client;
        
        public StorageFixture()
        {   
            Config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."))
                .AddEnvironmentVariables()
                .AddUserSecrets<StorageFixture>()
                .Build();
            
            _client = new BlobServiceClient(Config["ConnectionString"]);
        }

        public IConfiguration Config { get; private set; }

        public void Dispose()
        {
            var list = new List<BlobContainerItem>();
            list.AddRange(_client.GetBlobContainers());

            foreach (var container in list)
            {
                _client.DeleteBlobContainer(container.Name);
            }
        }
    }
}