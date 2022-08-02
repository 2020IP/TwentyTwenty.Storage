using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using Azure.Storage.Sas;
using Azure.Storage;

namespace TwentyTwenty.Storage.Azure
{
    public sealed class AzureStorageProvider : IStorageProvider
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IDictionary<string, string> _settings;
        public AzureStorageProvider(AzureProviderOptions options)
        {
            _blobServiceClient = new BlobServiceClient(options.ConnectionString);

            _settings = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                var splitted = options.ConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var nameValue in splitted)
                {
                    var splittedNameValue = nameValue.Split(new char[] { '=' }, 2);
                    _settings.Add(splittedNameValue[0], splittedNameValue[1]);
                }
            }
        }        

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            try
            {
                await _blobServiceClient.GetBlobContainerClient(containerName)
                    .GetBlobClient(blobName)
                    .DeleteIfExistsAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            try
            {
                await _blobServiceClient.GetBlobContainerClient(containerName)
                    .DeleteIfExistsAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }
        }

        public async Task CopyBlobAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName,
            string destinationBlobName = null)
        {
            var sourceContainer = _blobServiceClient.GetBlobContainerClient(sourceContainerName);
            var sourceBlob = sourceContainer.GetBlobClient(sourceBlobName);

            var destContainer = _blobServiceClient.GetBlobContainerClient(destinationContainerName);

            if (!await destContainer.ExistsAsync().ConfigureAwait(false))
            {
                var sourceProps = await sourceContainer.GetPropertiesAsync().ConfigureAwait(false);

                await destContainer.CreateIfNotExistsAsync(publicAccessType: sourceProps.Value.PublicAccess ?? PublicAccessType.None, metadata: sourceProps.Value.Metadata).ConfigureAwait(false);
            }

            var destBlob = destContainer.GetBlobClient(destinationBlobName ?? sourceBlobName);

            var operation = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri).ConfigureAwait(false);

            await operation.WaitForCompletionAsync().ConfigureAwait(false);

            var destProps = await destBlob.GetPropertiesAsync();
            if (destProps.Value.CopyStatus != CopyStatus.Success)
            {
                throw new Exception("Copy failed: " + destProps.Value.CopyStatus + ", " + destProps.Value.CopyStatusDescription);
            }
        }

        public async Task MoveBlobAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName,
            string destinationBlobName = null)
        {
            await CopyBlobAsync(sourceContainerName, sourceBlobName, destinationContainerName, destinationBlobName);
            await DeleteBlobAsync(sourceContainerName, sourceBlobName);
        }

        public async Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(blobName);

            try
            {
                var blobProps = await blob.GetPropertiesAsync().ConfigureAwait(false);
                var accessPolicy = await container.GetAccessPolicyAsync().ConfigureAwait(false);

                return new BlobDescriptor
                {
                    Name = blob.Name,
                    Container = containerName,
                    Url = blob.Uri.ToString(),
                    Security = accessPolicy.Value.BlobPublicAccess == PublicAccessType.None ? BlobSecurity.Private : BlobSecurity.Public,
                    ContentType = blobProps.Value.ContentType,
                    ContentMD5 = string.Join(string.Empty, blobProps.Value.ContentHash.Select(x => x.ToString("X2"))),
                    ETag = blobProps.Value.ETag.ToString(),
                    LastModified = blobProps.Value.LastModified,
                    Length = blobProps.Value.ContentLength,
                };
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            var blob = _blobServiceClient.GetBlobContainerClient(containerName)
                .GetBlobClient(blobName);

            try
            {
                return await blob.OpenReadAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName)
                .GetBlobClient(blobName)
                .Uri.ToString();
        }

        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, bool isDownload = false,
            string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            var blob = _blobServiceClient.GetBlobContainerClient(containerName)
                .GetBlobClient(blobName);

            var builder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = expiry
            };

            var p = access.ToPermissions();
            if (p != null)
            {
                builder.SetPermissions(p.Value);
            }

            if (isDownload)
            {
                builder.ContentDisposition = "attachment";
            }
            else
            {
                builder.ContentDisposition = "inline";
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                builder.ContentDisposition += ";filename=" + fileName;
            }

            if (!string.IsNullOrEmpty(contentType))
            {
                builder.ContentType = contentType;
            }

            var query = builder.ToSasQueryParameters(new StorageSharedKeyCredential(_settings["AccountName"], _settings["AccountKey"]));

            var uriBuilder = new UriBuilder(blob.Uri)
            {
                Query = query.ToString().TrimStart('?')
            };

            return uriBuilder.Uri.ToString();
        }

        public async Task<IList<BlobDescriptor>> ListBlobsAsync(string containerName)
        {
            var list = new List<BlobDescriptor>();
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var security = BlobSecurity.Public;

            try
            {
                var result = container.GetBlobsAsync();
                var iterator = result.GetAsyncEnumerator();

                while (await iterator.MoveNextAsync().ConfigureAwait(false))
                {
                    var blob = iterator.Current;

                    list.Add(new BlobDescriptor
                    {
                        Name = blob.Name,
                        Container = containerName,
                        Url = container.Uri.AbsoluteUri.ToString().TrimEnd('/') + "/" + blob.Name,
                        ContentType = blob.Properties.ContentType,
                        ContentMD5 = string.Join(string.Empty, blob.Properties.ContentHash.Select(x => x.ToString("X2"))),
                        ETag = blob.Properties.ETag.ToString(),
                        Length = blob.Properties.ContentLength ?? default,
                        LastModified = blob.Properties.LastModified,
                        Security = security,
                    });
                }
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }

            return list;
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source,
            BlobProperties properties = null, bool closeStream = true, long? length = null)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var props = properties ?? BlobProperties.Empty;
            var security = props.Security == BlobSecurity.Public ? PublicAccessType.Blob : PublicAccessType.None;

            try
            {
                var info = await container.CreateIfNotExistsAsync(publicAccessType: security).ConfigureAwait(false);
                var created = info != null;

                var blob = container.GetBlobClient(blobName);

                await blob.UploadAsync(source, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = props.ContentType,
                        ContentDisposition = props.ContentDisposition
                    },
                    Metadata = props.Metadata
                }).ConfigureAwait(false);

                // Check if container permission elevation is necessary
                if (!created)
                {
                    var accessPolicy = await container.GetAccessPolicyAsync().ConfigureAwait(false);

                    if (properties != null && properties.Security == BlobSecurity.Public && accessPolicy.Value.BlobPublicAccess == PublicAccessType.None)
                    {
                        await container.SetAccessPolicyAsync(security).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }
            finally
            {
                if (closeStream)
                {
                    source.Dispose();
                }
            }
        }

        public async Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(blobName);

            try
            {
                var props = await blob.GetPropertiesAsync().ConfigureAwait(false);

                await blob.SetHttpHeadersAsync(new BlobHttpHeaders
                {
                    ContentType = properties.ContentType,
                    ContentDisposition = properties.ContentDisposition
                }).ConfigureAwait(false);

                if (properties.Metadata != null)
                {
                    await blob.SetMetadataAsync(properties.Metadata).ConfigureAwait(false);
                }

                var accessPolicy = await container.GetAccessPolicyAsync().ConfigureAwait(false);

                // Elevate container permissions if necessary.
                if (properties.Security == BlobSecurity.Public && accessPolicy.Value.BlobPublicAccess == PublicAccessType.None)
                {
                    await container.SetAccessPolicyAsync(PublicAccessType.Blob).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                if (e.IsAzureStorageException())
                {
                    throw e.Convert();
                }
                throw;
            }
        }
    }
}