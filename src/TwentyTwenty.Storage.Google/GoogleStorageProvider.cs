using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google;
using Google.Apis.Upload;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Blob = Google.Apis.Storage.v1.Data.Object;
using PredefinedAcl = Google.Apis.Storage.v1.ObjectsResource.InsertMediaUpload.PredefinedAclEnum;
using Google.Apis.Requests;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Google.Cloud.Storage.V1;

namespace TwentyTwenty.Storage.Google
{
    public sealed class GoogleStorageProvider : IStorageProvider
    {
        private const string BlobNameRegex = @"(?<Container>[^/]+)/(?<Blob>.+)";
        private const string DefaultContentType = "application/octet-stream";
        private readonly StorageClient _client;
        private readonly string _bucket;
        private readonly string _serviceEmail;
        private readonly X509Certificate2 _certificate;

        public GoogleStorageProvider(GoogleCredential credential, GoogleProviderOptions options)
        {
            _client = StorageClient.Create(credential);

            _serviceEmail = options.Email;
            _bucket = options.Bucket;
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source, BlobProperties properties = null, bool closeStream = true)
        {
            try
            {
                await _client.UploadObjectAsync(_bucket, 
                    ObjectName(containerName, blobName), properties?.ContentType, source);
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            try
            {
                var stream = new MemoryStream();
                await _client.DownloadObjectAsync(_bucket, 
                    ObjectName(containerName, blobName), stream);
                
                stream.Seek(0, SeekOrigin.Begin);
                
                return stream;
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            throw new NotImplementedException();
        //     try
        //     {
        //         var obj = _storageService.GetObject(_bucket, ObjectName(containerName, blobName));
                
        //         return GetBlob(containerName, blobName).MediaLink;
        //     }
        //     catch (GoogleApiException gae)
        //     {
        //         throw Error(gae);
        //     }
        }

        // // TODO: Currently google only support adding a content disposition when uploading
        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, bool isDownload = false,
            string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            throw new NotImplementedException();
        //     var expiration = expiry.ToUnixTimeSeconds();
        //     //var disp = fileName != null ? "content-disposition:attachment;filename=\"" + fileName +"\"" : string.Empty;
        //     var verb = access == BlobUrlAccess.Read ? "GET" : "PUT";
        //     var urlSignature = SignString($"{verb}\n\n{contentType}\n{expiration}\n/{_bucket}/{containerName}/{blobName}");

        //     return $"https://storage.googleapis.com/{_bucket}/{containerName}/{blobName}?GoogleAccessId={_serviceEmail}&Expires={expiration}&Signature={WebUtility.UrlEncode(urlSignature)}";
        }

        public async Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            try
            {
                var obj = await _client.GetObjectAsync(_bucket, ObjectName(containerName, blobName));

                return GetBlobDescriptor(obj);
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
                return await _client.ListObjectsAsync(_bucket, containerName)
                    .Select(GetBlobDescriptor)
                    .ToList();
            }
            catch (GoogleApiException gae)
            {
                throw Error(gae);
            }
        }

        public Task DeleteBlobAsync(string containerName, string blobName)
        {
            throw new NotImplementedException();
        //     try
        //     {
        //         return _storageService.DeleteObjectAsync(_bucket, ObjectName(containerName, blobName));
        //     }
        //     catch (GoogleApiException gae)
        //     {
        //         throw Error(gae);
        //     }
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            throw new NotImplementedException();
        //     try
        //     {
        //         var batch = new BatchRequest(_storageService);

        //         foreach (var blob in await ListBlobsAsync(containerName))
        //         {
        //             batch.Queue<string>(_storageService.Objects.Delete(_bucket, $"{blob.Container}/{blob.Name}"),
        //                 (content, error, i, message) => { });
        //         }

        //         await batch.ExecuteAsync();
        //     }
        //     catch (GoogleApiException gae)
        //     {
        //         throw Error(gae);
        //     }
        }

        public Task CopyBlobAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName,
            string destinationBlobName = null)
        {
            throw new NotImplementedException();
        }

        public Task MoveBlobAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName,
            string destinationBlobName = null)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            throw new NotImplementedException();
        //     try
        //     {
        //         await UpdateRequest(containerName, blobName, properties).ExecuteAsync();
        //     }
        //     catch (GoogleApiException gae)
        //     {
        //         throw Error(gae);
        //     }
        }

        // #region Helpers

        // private ObjectsResource.InsertMediaUpload SaveRequest(string containerName, string blobName, Stream source, BlobProperties properties)
        // {
        //     var blob = CreateBlob(containerName, blobName, properties);

        //     var req = _storageService.Objects.Insert(blob, _bucket, source,
        //         properties?.ContentType ?? DefaultContentType);

        //     req.PredefinedAcl = properties?.Security == BlobSecurity.Public ? PredefinedAcl.PublicRead : PredefinedAcl.Private__;
            
        //     return req;
        // }

        // private ObjectsResource.UpdateRequest UpdateRequest(string containerName, string blobName, BlobProperties properties)
        // {
        //     var blob = CreateBlob(containerName, blobName, properties);
        //     var req = _storageService.Objects.Update(blob, _bucket, $"{containerName}/{blobName}");
        //     req.PredefinedAcl = properties?.Security == BlobSecurity.Public ? 
        //         ObjectsResource.UpdateRequest.PredefinedAclEnum.PublicRead : 
        //         ObjectsResource.UpdateRequest.PredefinedAclEnum.Private__;
        //     return req;
        // }

        private Blob CreateBlob(string containerName, string blobName, BlobProperties properties = null)
        {
            return new Blob
            {
                Name = $"{containerName}/{blobName}",
                ContentType = properties?.ContentType ?? DefaultContentType
            };
        }

        private string ObjectName(string containerName, string blobName)
            => $"{containerName}/{blobName}";

        private BlobDescriptor GetBlobDescriptor(Blob blob)
        {
            var match = Regex.Match(blob.Name, BlobNameRegex);
            if (!match.Success)
            {
                throw new InvalidOperationException("Unable to match blob name with regex; all blob names");
            }

            var blobDescriptor = new BlobDescriptor
            {
                Container = match.Groups["Container"].Value,
                ContentMD5 = blob.Md5Hash,
                ContentType = blob.ContentType,
                ETag = blob.ETag,
                LastModified = blob.Updated,
                Length = (long)blob.Size.GetValueOrDefault(),
                Name = match.Groups["Blob"].Value,
                Security = blob.Acl != null 
                    && blob.Acl.Any(acl => acl.Entity.ToLowerInvariant() == "allusers") ? BlobSecurity.Public : BlobSecurity.Private,
                Url = blob.MediaLink
            };

            return blobDescriptor;
        }

        // private ObjectsResource.ListRequest GetListBlobsRequest(string containerName)
        // {
        //     var req = _storageService.Objects.List(_bucket);
        //     req.Prefix = containerName;
        //     return req;
        // }

        private StorageException Error(GoogleApiException gae, int code = 1001, string message = null)
        {
            return new StorageException(new StorageError
            {
                Code = code,
                Message =
                    message ?? "Encountered an error when making a request to Google's Cloud API.",
                ProviderMessage = gae?.Message
            }, gae);
        }

        // private string SignString(string stringToSign)
        // {
        //     if (_certificate == null)
        //     {
        //         throw new Exception("Certificate not initialized");
        //     }

        //     var cp = new CspParameters(24, "Microsoft Enhanced RSA and AES Cryptographic Provider",
        //             ((RSACryptoServiceProvider)_certificate.PrivateKey).CspKeyContainerInfo.KeyContainerName);
        //     var provider = new RSACryptoServiceProvider(cp);
        //     var buffer = Encoding.UTF8.GetBytes(stringToSign);
        //     var signature = provider.SignData(buffer, "SHA256");
        //     return Convert.ToBase64String(signature);
        // }

        // #endregion
    }
}