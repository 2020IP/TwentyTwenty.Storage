using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Microsoft.Framework.Configuration;
using TwentyTwenty.Storage.Google;

namespace TwentyTwenty.Storage.Google.Test
{
    public class StorageFixture : IDisposable
    {
        public const string ContainerPrefix = "storagetest-";

        public StorageService Client { get; }

        public StorageFixture()
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(".")
                .AddEnvironmentVariables()
                .AddUserSecrets()
                .Build();

            var credential =
                new ServiceAccountCredential(new ServiceAccountCredential.Initializer(Config["GoogleEmail"])
                {
                    Scopes = new[] {StorageService.Scope.DevstorageFullControl}
                }.FromPrivateKey(Config["GooglePrivateKey"]));

            Client = new StorageService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Storage Test"
            });
        }

        public IConfiguration Config { get; private set; }

        public void Dispose()
        {
            var blobsToDelete = AsyncHelpers.RunSync(() => Client.Objects.List(Config["GoogleBucket"]).ExecuteAsync()).Items
                .WhereToListOrEmpty(b => b.Name.StartsWith(ContainerPrefix));

            foreach (var blob in blobsToDelete)
            {
                AsyncHelpers.RunSync(() => Client.Objects.Delete(Config["GoogleBucket"], blob.Name).ExecuteAsync());
            }
        }
    }
}
