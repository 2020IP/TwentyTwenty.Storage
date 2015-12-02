using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google;
using Google.Apis.Storage;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Upload;
using Newtonsoft.Json.Linq;
using TwentyTwenty.Storage;
using Blob = Google.Apis.Storage.v1.Data.Object;
using PredefinedAcl = Google.Apis.Storage.v1.ObjectsResource.InsertMediaUpload.PredefinedAclEnum;

namespace TwentyTwenty.Storage.Google
{
    public class GoogleStorageProvider : IStorageProvider
    {
        /// <summary>
        /// {0} - Container name
        /// {1} - Blob name
        /// </summary>
        private const string ContainerBlobFormat = @"{0}/{1}";

        private const string BlobNameRegex = @"(?<Container>[^/]+)/(?<Blob>.+)";

        private const string DefaultContentType = "application/octet-stream";

        private StorageService _storageService;

        private string _bucket;

        public GoogleStorageProvider(GoogleProviderOptions options)
        {
            _storageService = options.GoogleStorageClient;
            _bucket = options.Bucket;
        }

        public void SaveBlobStream(string continerName, string blobName, Stream source, BlobProperties properties = null)
        {
            try
            {
                var response = SaveRequest(continerName, blobName, source, properties).Upload();

                //Google's errors are all generic, so there's really no way that I currently know to detect what went wrong exactly.
                if (response.Status == UploadStatus.Failed)
                {
                    throw Error(response.Exception as GoogleApiException, message: "There was an error uploading to Google Cloud Storage");
                }
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public Task SaveBlobStreamAsync(string continerName, string blobName, Stream source, BlobProperties properties = null)
        {
            try
            {
                return SaveBlobAsync(SaveRequest(continerName, blobName, source, properties));
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        private async Task SaveBlobAsync(ObjectsResource.InsertMediaUpload req)
        {
            try
            {
                var response = await req.UploadAsync();

                if (response.Status == UploadStatus.Failed)
                {
                    throw Error(response.Exception as GoogleApiException, message: "There was an error uploading to Google Cloud Storage");
                }
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public Stream GetBlobStream(string containerName, string blobName)
        {
            try
            {
                return AsyncHelpers.RunSync(() => _storageService.HttpClient.GetStreamAsync(GetBlob(containerName, blobName).MediaLink));
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            try
            {
                return _storageService.HttpClient.GetStreamAsync(GetBlob(containerName, blobName).MediaLink);
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            try
            {
                return GetBlob(containerName, blobName).MediaLink;
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, bool isDownload = false,
            string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            throw new NotImplementedException();
        }

        public BlobDescriptor GetBlobDescriptor(string containerName, string blobName)
        {
            try
            {
                return GetBlobDescriptor(GetBlob(containerName, blobName));
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public async Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            try
            {
                return GetBlobDescriptor(await GetBlobAsync(containerName, blobName));
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public IList<BlobDescriptor> ListBlobs(string containerName)
        {
            try
            {
                return GetListBlobsRequest(containerName).Execute().Items.SelectToListOrEmpty(GetBlobDescriptor);
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public async Task<IList<BlobDescriptor>> ListBlobsAsync(string containerName)
        {
            try
            {
                return (await GetListBlobsRequest(containerName).ExecuteAsync()).Items.SelectToListOrEmpty(GetBlobDescriptor);
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            try
            {
                _storageService.Objects.Delete(_bucket, string.Format(ContainerBlobFormat, containerName, blobName)).Execute();
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public Task DeleteBlobAsync(string containerName, string blobName)
        {
            try
            {
                return _storageService.Objects.Delete(_bucket, string.Format(ContainerBlobFormat, containerName, blobName)).ExecuteAsync();
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public void DeleteContainer(string containerName)
        {
            try
            {
                var containerBlobs = ListBlobs(containerName);

                //TODO:  Parallel.ForEach or something similarly performing.  To use that would currently change my project targets, and I don't think I want to do that.  I just have a feeling this is not going to perform very well...
                foreach (var blob in containerBlobs)
                {
                    _storageService.Objects.Delete(_bucket, string.Format(ContainerBlobFormat, blob.Container, blob.Name)).Execute();
                }

            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }

        }

        public async Task DeleteContainerAsync(string containerName)
        {
            try
            {
                var containerBlobs = await ListBlobsAsync(containerName);
                var tasks = containerBlobs.Select(blob => _storageService.Objects.Delete(_bucket,
                    string.Format(ContainerBlobFormat, blob.Container, blob.Name)).ExecuteAsync());
                //Something tells me this is not the best way to do this.
                await Task.WhenAll(tasks);
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public void UpdateBlobProperties(string containerName, string blobName, BlobProperties properties)
        {
            try
            {
                UpdateRequest(containerName, blobName, properties).Execute();
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            try
            {
                return UpdateRequest(containerName, blobName, properties).ExecuteAsync();
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        #region Helpers

        private ObjectsResource.InsertMediaUpload SaveRequest(string containerName, string blobName, Stream source, BlobProperties properties)
        {
            var blob = CreateBlob(containerName, blobName, properties);

            var req = _storageService.Objects.Insert(blob, _bucket, source,
                properties?.ContentType ?? DefaultContentType);

            req.PredefinedAcl = properties?.Security == BlobSecurity.Public ? PredefinedAcl.PublicRead : PredefinedAcl.Private__;
            
            return req;
        }

        private ObjectsResource.UpdateRequest UpdateRequest(string containerName, string blobName, BlobProperties properties)
        {
            var blob = CreateBlob(containerName, blobName, properties);
            var req = _storageService.Objects.Update(blob, _bucket, string.Format(ContainerBlobFormat, containerName, blobName));
            req.PredefinedAcl = properties?.Security == BlobSecurity.Public ? ObjectsResource.UpdateRequest.PredefinedAclEnum.PublicRead : ObjectsResource.UpdateRequest.PredefinedAclEnum.Private__;
            return req;
        }

        private Blob CreateBlob(string containerName, string blobName, BlobProperties properties = null)
        {
            return new Blob
            {
                Name = string.Format(ContainerBlobFormat, containerName, blobName),
                ContentType = properties?.ContentType ?? DefaultContentType
            };
        }

        private Task<Blob> GetBlobAsync(string containerName, string blobName, DateTimeOffset? endEx = null, bool isDownload = false, string optionalFileName = null)
        {
            //TODO:  Use the optional fields
            //TODO:  Verify that unless the optional fields are provided, the URL provided will NOT be SAS.
            var req = _storageService.Objects.Get(_bucket, string.Format(ContainerBlobFormat, containerName, blobName));
            try
            {
                return req.ExecuteAsync();
            }
            catch (GoogleApiException e)
            {
                if (e.Message.Contains("404"))
                {
                    return null;
                }

                throw;
            }
        }

        private Blob GetBlob(string containerName, string blobName, DateTimeOffset? endEx = null,
            bool isDownload = false, string optionalFileName = null)
        {
            //TODO:  Use the optional fields
            //TODO:  Verify that unless the optional fields are provided, the URL provided will NOT be SAS.
            var req = _storageService.Objects.Get(_bucket, string.Format(ContainerBlobFormat, containerName, blobName));
            req.Projection = ObjectsResource.GetRequest.ProjectionEnum.Full;

            try
            {
                return req.Execute();
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
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
                Name = match.Groups["Blob"].Value,
                Security = blob.Acl != null && blob.Acl.Any(acl => acl.Entity.ToLowerInvariant() == "allusers") ? BlobSecurity.Public : BlobSecurity.Private,
                Url = blob.MediaLink
            };

            return blobDescriptor;
        }

        private ObjectsResource.ListRequest GetListBlobsRequest(string containerName)
        {
            var req = _storageService.Objects.List(_bucket);
            req.Prefix = containerName;
            return req;
        }

        private StorageException Error(GoogleApiException gae, int code = 1001, string message = null)
        {
            return new StorageException(new StorageError()
            {
                Code = code,
                Message =
                    message ?? "Encountered an error when making a request to Google's Cloud API.  Unfortunately, as of the time this is being developed, Google's error messages (when using the .NET client library) are not very informative, and do not usually provide any clues as to what may have gone wrong.",
                ProviderMessage = gae?.Message
            }, gae);
        }

        #endregion
    }
}
