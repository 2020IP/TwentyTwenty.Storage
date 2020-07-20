using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using System.Net.Http.Headers;

namespace TwentyTwenty.Storage.Amazon
{
    // NOTE: S3 doesn't support getting the MD5 hash of objects
    //          It unreliably returns the MD5 in the ETAG header
    //          which is what is currently being put into the ContentMD5 property

    public sealed class AmazonStorageProvider : IStorageProvider
    {
        private const string DefaultServiceUrl = "https://s3.amazonaws.com";
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucket;
        private readonly string _serverSideEncryptionMethod;
        private readonly string _serviceUrl;
        private readonly long _chunkedThreshold;

        public AmazonStorageProvider(AmazonProviderOptions options)
        {
            _serviceUrl = string.IsNullOrEmpty(options.ServiceUrl) ? DefaultServiceUrl : options.ServiceUrl;
            _bucket = options.Bucket;
            _serverSideEncryptionMethod = options.ServerSideEncryptionMethod;
            _chunkedThreshold = options.ChunkedUploadThreshold;

            var S3Config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                Timeout = options.Timeout ?? ClientConfig.MaxTimeout,
            };

            _s3Client = new AmazonS3Client(ReadAwsCredentials(options), S3Config);            
        }

        private AWSCredentials ReadAwsCredentials(AmazonProviderOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ProfileName))
            {
                var credentialProfileStoreChain = new CredentialProfileStoreChain();
                if (credentialProfileStoreChain.TryGetAWSCredentials(options.ProfileName, out AWSCredentials defaultCredentials))
                {
                    return defaultCredentials;
                }

                throw new AmazonClientException("Unable to find a default profile in CredentialProfileStoreChain.");
            }

            if (!string.IsNullOrEmpty(options.PublicKey) && !string.IsNullOrWhiteSpace(options.SecretKey))
            {
                return new BasicAWSCredentials(options.PublicKey, options.SecretKey);
            }
            
            return FallbackCredentialsFactory.GetCredentials();
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var key = GenerateKeyName(containerName, blobName);

            var objectDeleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = key
            };

            try
            {
                await _s3Client.DeleteObjectAsync(objectDeleteRequest);
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public async Task CopyBlobAsync(string sourceContainerName, string sourceBlobName,
            string destinationContainerName, string destinationBlobName = null)
        {
            if (string.IsNullOrEmpty(sourceContainerName))
            {
                throw new StorageException(StorageErrorCode.InvalidName, $"Invalid {nameof(sourceContainerName)}");
            }
            if (string.IsNullOrEmpty(sourceBlobName))
            {
                throw new StorageException(StorageErrorCode.InvalidName, $"Invalid {nameof(sourceBlobName)}");
            }
            if (string.IsNullOrEmpty(destinationContainerName))
            {
                throw new StorageException(StorageErrorCode.InvalidName, $"Invalid {nameof(destinationContainerName)}");
            }
            if (destinationBlobName == string.Empty)
            {
                throw new StorageException(StorageErrorCode.InvalidName, $"Invalid {nameof(destinationBlobName)}");
            }

            var sourceKey = GenerateKeyName(sourceContainerName, sourceBlobName);
            var destinationKey = GenerateKeyName(destinationContainerName, destinationBlobName ?? sourceBlobName);

            try
            {
                // Get the size of the object.
                var metadataRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucket,
                    Key = sourceKey,
                };

                AmazonWebServiceResponse response;
                var metadataResponse = await _s3Client.GetObjectMetadataAsync(metadataRequest);

                var objectSize = metadataResponse.ContentLength; // Length in bytes.
                var limit = 5 * (long)Math.Pow(2, 30); // CopyObject size limit 5 GB.

                if (objectSize >= limit)
                {
                    var request = new InitiateMultipartUploadRequest
                    {
                        BucketName = _bucket,
                        Key = destinationKey,
                        ContentType = metadataResponse.Headers.ContentType,
                        // CannedACL = metadataResponse.Headers.. GetCannedACL(properties),
                        ServerSideEncryptionMethod = metadataResponse.ServerSideEncryptionMethod,
                    };
                    request.Headers.ContentDisposition = metadataResponse.Headers.ContentDisposition;
                    request.Metadata.AddMetadata(metadataResponse.Metadata.ToMetadata());
                    
                    response = await MultipartCopy(sourceKey, destinationKey, objectSize, request);
                }
                else
                {
                    var request = new CopyObjectRequest
                    {
                        SourceBucket = _bucket,
                        SourceKey = sourceKey,
                        DestinationBucket = _bucket,
                        DestinationKey = destinationKey,
                        ServerSideEncryptionMethod = _serverSideEncryptionMethod
                    };

                    response = await _s3Client.CopyObjectAsync(request);
                }

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new StorageException(StorageErrorCode.GenericException, "Copy failed.");
                }
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public async Task MoveBlobAsync(string sourceContainerName, string sourceBlobName,
            string destinationContainerName, string destinationBlobName = null)
        {
            await CopyBlobAsync(sourceContainerName, sourceBlobName, destinationContainerName, destinationBlobName);
            await DeleteBlobAsync(sourceContainerName, sourceBlobName);
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            var objectsRequest = new ListObjectsRequest
            {
                BucketName = _bucket,
                Prefix = containerName,
                MaxKeys = 100000
            };

            var keys = new List<KeyVersion>();

            try
            {
                do
                {
                    var objectsResponse = await _s3Client.ListObjectsAsync(objectsRequest);

                    keys.AddRange(objectsResponse.S3Objects
                        .Select(x => new KeyVersion { Key = x.Key, VersionId = null }));

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
                    var objectsDeleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _bucket,
                        Objects = keys
                    };

                    await _s3Client.DeleteObjectsAsync(objectsDeleteRequest);
                }
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public async Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            var key = GenerateKeyName(containerName, blobName);

            try
            {
                var objectMetaRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucket,
                    Key = key
                };

                var objectMetaResponse = await _s3Client.GetObjectMetadataAsync(objectMetaRequest);

                var objectAclRequest = new GetACLRequest
                {
                    BucketName = _bucket,
                    Key = key
                };

                var objectAclResponse = await _s3Client.GetACLAsync(objectAclRequest);
                var isPublic = objectAclResponse.AccessControlList.Grants.Any(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers");

                return new BlobDescriptor
                {
                    Name = blobName,
                    Container = containerName,
                    Length = objectMetaResponse.Headers.ContentLength,
                    ETag = objectMetaResponse.ETag,
                    ContentMD5 = objectMetaResponse.ETag,
                    ContentType = objectMetaResponse.Headers.ContentType,
                    ContentDisposition = objectMetaResponse.Headers.ContentDisposition,
                    LastModified = objectMetaResponse.LastModified,
                    Security = isPublic ? BlobSecurity.Public : BlobSecurity.Private,
                    Metadata = objectMetaResponse.Metadata.ToMetadata(),
                };
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            try
            {
                return await _s3Client.GetObjectStreamAsync(_bucket, GenerateKeyName(containerName, blobName), null);
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            return $"{_serviceUrl}/{_bucket}/{GenerateKeyName(containerName, blobName)}";
        }

        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, bool isDownload = false,
            string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            var headers = new ResponseHeaderOverrides();

            ContentDispositionHeaderValue cdHeader;
            if (isDownload)
            {
                cdHeader = new ContentDispositionHeaderValue("attachment");
            }
            else
            {
                cdHeader = new ContentDispositionHeaderValue("inline");
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                cdHeader.FileNameStar = fileName;
            }

            headers.ContentDisposition = cdHeader.ToString();

            if (!string.IsNullOrEmpty(contentType))
            {
                headers.ContentType = contentType;
            }

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = GenerateKeyName(containerName, blobName),
                Expires = expiry.UtcDateTime,
                ResponseHeaderOverrides = headers,
                Verb = access == BlobUrlAccess.Read ? HttpVerb.GET : HttpVerb.PUT
            };

            if (!string.IsNullOrEmpty(_serverSideEncryptionMethod))
            {
                urlRequest.ServerSideEncryptionMethod = _serverSideEncryptionMethod;
            }

            try
            {
                return _s3Client.GetPreSignedURL(urlRequest);
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public async Task<IList<BlobDescriptor>> ListBlobsAsync(string containerName)
        {
            var descriptors = new List<BlobDescriptor>();

            var objectsRequest = new ListObjectsRequest
            {
                BucketName = _bucket,
                Prefix = containerName,
                MaxKeys = 100000
            };

            try
            {
                do
                {
                    var objectsResponse = await _s3Client.ListObjectsAsync(objectsRequest);

                    foreach (S3Object entry in objectsResponse.S3Objects)
                    {
                        var objectMetaRequest = new GetObjectMetadataRequest
                        {
                            BucketName = _bucket,
                            Key = entry.Key
                        };

                        var objectMetaResponse = await _s3Client.GetObjectMetadataAsync(objectMetaRequest);

                        var objectAclRequest = new GetACLRequest
                        {
                            BucketName = _bucket,
                            Key = entry.Key
                        };

                        var objectAclResponse = await _s3Client.GetACLAsync(objectAclRequest);
                        var isPublic = objectAclResponse.AccessControlList.Grants.Any(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers");

                        descriptors.Add(new BlobDescriptor
                        {
                            Name = entry.Key.Remove(0, containerName.Length + 1),
                            Container = containerName,
                            Length = entry.Size,
                            ETag = entry.ETag,
                            ContentMD5 = entry.ETag,
                            ContentType = objectMetaResponse.Headers.ContentType,
                            LastModified = entry.LastModified,
                            Security = isPublic ? BlobSecurity.Public : BlobSecurity.Private,
                            ContentDisposition = objectMetaResponse.Headers.ContentDisposition,
                            Metadata = objectMetaResponse.Metadata.ToMetadata(),
                        });
                    }

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

                return descriptors;
            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source, 
            BlobProperties properties = null, bool closeStream = true, long? length = null)
        {
            length = source.CanSeek ? source.Length : length;

            // PutObject supports a max of 5GB.
            var threshold = Math.Min(_chunkedThreshold, 5000000000);

            if (length.HasValue && length.Value >= threshold)
            {
                var fileTransferUtilityRequest = CreateChunkedUpload(containerName, blobName, source, properties, closeStream, length);
                try
                {
                    await new TransferUtility(_s3Client).UploadAsync(fileTransferUtilityRequest);
                }
                catch (AmazonS3Exception asex)
                {
                    throw asex.ToStorageException();
                }
            }
            else
            {
                var putRequest = CreateUpload(containerName, blobName, source, properties, closeStream, length);

                try
                {
                    await _s3Client.PutObjectAsync(putRequest);
                }
                catch (AmazonS3Exception asex)
                {
                    throw asex.ToStorageException();
                }
            }
        }

        public async Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            var key = GenerateKeyName(containerName, blobName);
            try
            {
                // Get the size of the object.
                var metadataRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucket,
                    Key = key,
                };

                var metadataResponse = await _s3Client.GetObjectMetadataAsync(metadataRequest);

                var objectSize = metadataResponse.ContentLength; // Length in bytes.
                var limit = 5 * (long)Math.Pow(2, 30); // CopyObject size limit 5 GB.

                if (objectSize >= limit)
                {
                    var request = new InitiateMultipartUploadRequest
                    {
                        BucketName = _bucket,
                        Key = key,
                        ContentType = properties?.ContentType,
                        CannedACL = GetCannedACL(properties),
                        ServerSideEncryptionMethod = _serverSideEncryptionMethod
                    };
                    request.Headers.ContentDisposition = properties.ContentDisposition;
                    request.Metadata.AddMetadata(properties?.Metadata);
                    
                    var completeUploadResponse = await MultipartCopy(key, key, objectSize, request);
                }
                else
                {
                    var updateRequest = CreateUpdateRequest(containerName, blobName, properties);
                    await _s3Client.CopyObjectAsync(updateRequest);
                }

            }
            catch (AmazonS3Exception asex)
            {
                throw asex.ToStorageException();
            }
        }

        private S3CannedACL GetCannedACL(BlobProperties properties)
            => properties?.Security == BlobSecurity.Public ? S3CannedACL.PublicRead : S3CannedACL.Private;

        private static string GenerateKeyName(string containerName, string blobName) 
            => string.IsNullOrWhiteSpace(containerName) ? blobName : $"{containerName}/{blobName}";

        private CopyObjectRequest CreateUpdateRequest(string containerName, string blobName, BlobProperties properties)
        {
            var updateRequest = new CopyObjectRequest
            {
                SourceBucket = _bucket,
                SourceKey = GenerateKeyName(containerName, blobName),
                DestinationBucket = _bucket,
                DestinationKey = GenerateKeyName(containerName, blobName),
                ContentType = properties?.ContentType,
                CannedACL = GetCannedACL(properties),
                MetadataDirective = S3MetadataDirective.REPLACE,
                ServerSideEncryptionMethod = _serverSideEncryptionMethod
            };
            updateRequest.Headers.ContentDisposition = properties.ContentDisposition;
            updateRequest.Metadata.AddMetadata(properties?.Metadata);

            return updateRequest;
        }

        private TransferUtilityUploadRequest CreateChunkedUpload(string containerName, string blobName, Stream source, 
            BlobProperties properties, bool closeStream, long? length = null)
        {
            var fileTransferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucket,
                InputStream = source,
                PartSize = 6291456,
                Key = GenerateKeyName(containerName, blobName),
                ContentType = properties?.ContentType,
                CannedACL = GetCannedACL(properties),
                AutoCloseStream = closeStream,
                ServerSideEncryptionMethod = _serverSideEncryptionMethod
            };
            fileTransferUtilityRequest.Headers.ContentDisposition = properties?.ContentDisposition;
            fileTransferUtilityRequest.Metadata.AddMetadata(properties?.Metadata);

            if (length.HasValue)
            {
                fileTransferUtilityRequest.Headers.ContentLength = length.Value;
            }

            return fileTransferUtilityRequest;
        }

        private PutObjectRequest CreateUpload(string containerName, string blobName, Stream source, 
            BlobProperties properties, bool closeStream, long? length = null)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = GenerateKeyName(containerName, blobName),
                InputStream = source,
                ContentType = properties?.ContentType,
                CannedACL = GetCannedACL(properties),
                AutoCloseStream = closeStream,
                ServerSideEncryptionMethod = _serverSideEncryptionMethod
            };
            putRequest.Headers.ContentDisposition = properties?.ContentDisposition;
            putRequest.Metadata.AddMetadata(properties?.Metadata);

            if (length.HasValue)
            {
                putRequest.Headers.ContentLength = length.Value;
            }

            return putRequest;
        }

        private async Task<CompleteMultipartUploadResponse> MultipartCopy(string sourceKey, string destinationKey, 
            long objectSize, InitiateMultipartUploadRequest initiateRequest)
        {
            var copyResponses = new List<CopyPartResponse>();
            var partSize = 5 * (long)Math.Pow(2, 20); // Part size is 5 MB.
                    
            // Initiate the upload.                    
            var initResponse = await _s3Client.InitiateMultipartUploadAsync(initiateRequest);

            long bytePosition = 0;
            for (int i = 1; bytePosition < objectSize; i++)
            {
                var copyRequest = new CopyPartRequest
                {
                    DestinationBucket = _bucket,
                    DestinationKey = destinationKey,
                    SourceBucket = _bucket,
                    SourceKey = sourceKey,
                    UploadId = initResponse.UploadId,
                    FirstByte = bytePosition,
                    LastByte = bytePosition + partSize - 1 >= objectSize ? objectSize - 1 : bytePosition + partSize - 1,
                    PartNumber = i,
                };

                copyResponses.Add(await _s3Client.CopyPartAsync(copyRequest));

                bytePosition += partSize;
            }

            // Set up to complete the copy.
            var completeRequest = new CompleteMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = destinationKey,
                UploadId = initResponse.UploadId
            };
            completeRequest.AddPartETags(copyResponses);

            // Complete the copy.
            return await _s3Client.CompleteMultipartUploadAsync(completeRequest);
        }
    }
}