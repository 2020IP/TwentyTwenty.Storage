using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Storage;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Upload;
using TwentyTwenty.Storage;
using Blob = Google.Apis.Storage.v1.Data.Object;

namespace TwentyTwenty.Storage.Google
{
    public class GoogleStorageProvider : IStorageProvider
    {
        /// <summary>
        /// For blobs which have a "public" ACL.
        /// </summary>
        private readonly ObjectAccessControl PublicAcl = new ObjectAccessControl { Entity = "allUsers", Role = "READER" };

        /// <summary>
        /// {0} - Container name
        /// {1} - Blob name
        /// </summary>
        private const string ContainerBlobFormat = @"{0}/{1}";

        private const string BlobNameRegex = @"(?<Container>[^/]+/";

        private const string DefaultContentType = "application/octet-stream";

        private StorageService _storageService;
        private string _bucketName;

        public GoogleStorageProvider(StorageService service, string bucketName)
        {
            _storageService = service;
            _bucketName = bucketName;
        }

        private Blob CreateBlob(string containerName, string blobName, BlobProperties properties = null)
        {
            return new Blob
            {
                Name = string.Format(ContainerBlobFormat, containerName, blobName),

                //TODO:  Need to determine that ACL defaults to the bucket ACL or to the account, in the case that it does, this will work just fine (leaving Acl null if private)
                Acl = properties?.Security == BlobSecurity.Public ? new List<ObjectAccessControl> { PublicAcl } : null
            };
        }

        private Task<Blob> GetBlobAsync(string containerName, string blobName, DateTimeOffset? endEx = null, bool isDownload = false, string optionalFileName = null)
        {
            //TODO:  Use the optional fields
            //TODO:  Verify that unless the optional fields are provided, the URL provided will NOT be SAS.
            var req = _storageService.Objects.Get(_bucketName, string.Format(ContainerBlobFormat, containerName, blobName));
            return req.ExecuteAsync();
        }

        private Blob GetBlob(string containerName, string blobName, DateTimeOffset? endEx = null,
            bool isDownload = false, string optionalFileName = null)
        {
            //TODO:  Use the optional fields
            //TODO:  Verify that unless the optional fields are provided, the URL provided will NOT be SAS.
            var req = _storageService.Objects.Get(_bucketName, string.Format(ContainerBlobFormat, containerName, blobName));
            return req.Execute();
        }

        private BlobDescriptor GetBlobDescriptor(Blob blob)
        {
            var match = Regex.Match(blob.Name, BlobNameRegex);
            if (!match.Success)
            {
                throw new Exception("TODO:  Write in this Exception and use the proper exception type.");
            }

            var blobDescriptor = new BlobDescriptor
            {
                Container = match.Groups["Container"].Value,
                ContentMD5 = blob.Md5Hash,
                ContentType = blob.ContentType,
                ETag = blob.ETag,
                LastModified = DateTimeOffset.Parse(blob.UpdatedRaw),
                Length = Convert.ToInt64(blob.Size),
                Name = match.Groups["Container"].Value,
                Security = blob.Acl.Any() ? BlobSecurity.Private : BlobSecurity.Public,
                Url = blob.MediaLink
            };

            return blobDescriptor;
        }

        public void SaveBlobStream(string continerName, string blobName, Stream source, BlobProperties properties = null)
        {
            var blob = CreateBlob(continerName, blobName, properties);
            //Falls apart when content type is null.   Probably need to set a default one here.
            var something = _storageService.Objects.Insert(blob, _bucketName, source, properties?.ContentType ?? DefaultContentType).Upload();

            //TODO:  add useful error codes for the known error statuses returned from Google.
            if (something.Status == UploadStatus.Failed)
            {
                int code;

                switch (something.Exception.HResult)
                {
                    case -2146233079:
                        code = 1002;
                        break;

                    case -2147467261:
                        code = 1003;
                        break;

                    default:
                        code = 1001;
                        break;
                }

                throw new StorageException(
                    new StorageError
                    {
                        Code = code,
                        Message = "There was an error uploading to Google Cloud Storage",
                        ProviderMessage = something.ToString()
                    }, something.Exception);
            }
        }

        public Task SaveBlobStreamAsync(string continerName, string blobName, Stream source, BlobProperties properties = null)
        {
            var blob = CreateBlob(continerName, blobName, properties);
            return _storageService.Objects.Insert(blob, _bucketName, source, properties?.ContentType).UploadAsync();
        }

        public Stream GetBlobStream(string containerName, string blobName)
        {
            //TODO:  Use the proper synchronous way of calling async methods here.
            return _storageService.HttpClient.GetStreamAsync(GetBlob(containerName, blobName).MediaLink).Result;
        }

        public Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            return _storageService.HttpClient.GetStreamAsync(GetBlob(containerName, blobName).MediaLink);
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            return GetBlob(containerName, blobName).MediaLink;
        }

        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, bool isDownload = false,
            string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            throw new NotImplementedException();
        }

        public BlobDescriptor GetBlobDescriptor(string containerName, string blobName)
        {
            return GetBlobDescriptor(GetBlob(containerName, blobName));
        }

        public async Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            return GetBlobDescriptor(await GetBlobAsync(containerName, blobName));
        }

        public IList<BlobDescriptor> ListBlobs(string containerName)
        {
            //TODO:  Make sure there is no way to specify this prefix filter via the API library...
            return _storageService.Objects.List(_bucketName).Execute().Items
                .Where(b => b.Name.StartsWith(containerName + "/"))
                .Select(GetBlobDescriptor).ToList();
        }

        public async Task<IList<BlobDescriptor>> ListBlobsAsync(string containerName)
        {
            return (await _storageService.Objects.List(_bucketName).ExecuteAsync()).Items
                .Where(b => b.Name.StartsWith(containerName + "/"))
                .Select(GetBlobDescriptor).ToList();
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            _storageService.Objects.Delete(_bucketName, string.Format(ContainerBlobFormat, containerName, blobName))
                .Execute();
        }

        public Task DeleteBlobAsync(string containerName, string blobName)
        {
            return _storageService.Objects.Delete(_bucketName, string.Format(ContainerBlobFormat, containerName, blobName)).ExecuteAsync();
        }

        public void DeleteContainer(string containerName)
        {
            var containerBlobs = ListBlobs(containerName);

            //TODO:  Parallel.ForEach or something similarly performing.  To use that would currently change my project targets, and I don't think I want to do that.  I just have a feeling this is not going to perform very well...
            foreach (var blob in containerBlobs)
            {
                _storageService.Objects.Delete(_bucketName, string.Format(ContainerBlobFormat, blob.Container, blob.Name)).Execute();
            }
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            var containerBlobs = await ListBlobsAsync(containerName);
            var tasks = containerBlobs.Select(blob => _storageService.Objects.Delete(_bucketName,
                string.Format(ContainerBlobFormat, blob.Container, blob.Name)).ExecuteAsync());
            //Something tells me this is not the best way to do this.
            await Task.WhenAll(tasks);
        }

        public void UpdateBlobProperties(string containerName, string blobName, BlobProperties properties)
        {
            var blob = CreateBlob(containerName, blobName, properties);
            _storageService.Objects.Update(blob, _bucketName, string.Format(ContainerBlobFormat, containerName, blobName)).Execute();
        }

        public Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            var blob = CreateBlob(containerName, blobName, properties);
            return _storageService.Objects.Update(blob, _bucketName, string.Format(ContainerBlobFormat, containerName, blobName)).ExecuteAsync();
        }
    }
}
