using System;
using System.IO;
using System.Text;
using System.Linq;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace TwentyTwenty.Storage.Google.Test
{
    public sealed class StorageFixture : IDisposable
    {
        public const string ContainerPrefix = "storagetest-";
        public readonly StorageClient _client;
        public readonly GoogleCredential _credential;

        public StorageFixture()
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."))
                .AddEnvironmentVariables()
                .AddUserSecrets<StorageFixture>()
                .Build();

            var credJsonBytes = Convert.FromBase64String(Config["GoogleCredJsonBase64"]);
            var credJson = Encoding.UTF8.GetString(credJsonBytes);
            _credential = GoogleCredential.FromJson(credJson);

            _client = StorageClient.Create(_credential);
        }

        public IConfiguration Config { get; private set; }

        public void Dispose()
        {
            var objectsToDelete = _client.ListObjects(Config["GoogleBucket"], ContainerPrefix);

            foreach (var obj in objectsToDelete)
            {
                _client.DeleteObject(Config["GoogleBucket"], obj.Name);
            }
        }
    }
}
