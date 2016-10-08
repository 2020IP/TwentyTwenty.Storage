using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TwentyTwenty.Storage.Amazon
{
    // NOTE: S3 doesn't support getting the MD5 hash of objects
    //          It unreliably returns the MD5 in the ETAG header
    //          which is what is currently being put into the ContentMD5 property

    public sealed class AmazonStorageProvider : IStorageProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucket;
        private readonly string _serviceUrl = "https://s3.amazonaws.com";

        public AmazonStorageProvider(AmazonProviderOptions options)
        {
            var S3Config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl
            };

            _bucket = options.Bucket;
            _s3Client = new AmazonS3Client(options.PublicKey, options.SecretKey, S3Config);
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            var key = GenerateKeyName(containerName, blobName);

            var objectDeleteRequest = new DeleteObjectRequest()
            {
                BucketName = _bucket,
                Key = key
            };

            try
            {
                AsyncHelpers.RunSync(() => _s3Client.DeleteObjectAsync(objectDeleteRequest));
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var key = GenerateKeyName(containerName, blobName);

            var objectDeleteRequest = new DeleteObjectRequest()
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
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public void DeleteContainer(string containerName)
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
                    var objectsResponse = AsyncHelpers.RunSync(() => _s3Client.ListObjectsAsync(objectsRequest));

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
                        BucketName = _bucket,
                        Objects = keys
                    };

                    AsyncHelpers.RunSync(() => _s3Client.DeleteObjectsAsync(objectsDeleteRequest));
                }
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
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
                        BucketName = _bucket,
                        Objects = keys
                    };

                    await _s3Client.DeleteObjectsAsync(objectsDeleteRequest);
                }
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public BlobDescriptor GetBlobDescriptor(string containerName, string blobName)
        {
            var key = GenerateKeyName(containerName, blobName);

            try
            {
                var objectMetaRequest = new GetObjectMetadataRequest()
                {
                    BucketName = _bucket,
                    Key = key
                };

                var objectMetaResponse = AsyncHelpers.RunSync(() => _s3Client.GetObjectMetadataAsync(objectMetaRequest));

                var objectAclRequest = new GetACLRequest()
                {
                    BucketName = _bucket,
                    Key = key
                };

                var objectAclResponse = AsyncHelpers.RunSync(() => _s3Client.GetACLAsync(objectAclRequest));
                var isPublic = objectAclResponse.AccessControlList.Grants
                    .Where(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers").Count() > 0;

                return new BlobDescriptor
                {
                    Name = blobName,
                    Container = containerName,
                    Length = objectMetaResponse.Headers.ContentLength,
                    ETag = objectMetaResponse.ETag,
                    ContentMD5 = objectMetaResponse.ETag,
                    ContentType = objectMetaResponse.Headers.ContentType,
                    LastModified = objectMetaResponse.LastModified,
                    Security = isPublic ? BlobSecurity.Public : BlobSecurity.Private
                };
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public async Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            var key = GenerateKeyName(containerName, blobName);

            try
            {
                var objectMetaRequest = new GetObjectMetadataRequest()
                {
                    BucketName = _bucket,
                    Key = key
                };

                var objectMetaResponse = await _s3Client.GetObjectMetadataAsync(objectMetaRequest);

                var objectAclRequest = new GetACLRequest()
                {
                    BucketName = _bucket,
                    Key = key
                };

                var objectAclResponse = await _s3Client.GetACLAsync(objectAclRequest);
                var isPublic = objectAclResponse.AccessControlList.Grants
                    .Where(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers").Count() > 0;

                return new BlobDescriptor
                {
                    Name = blobName,
                    Container = containerName,
                    Length = objectMetaResponse.Headers.ContentLength,
                    ETag = objectMetaResponse.ETag,
                    ContentMD5 = objectMetaResponse.ETag,
                    ContentType = objectMetaResponse.Headers.ContentType,
                    LastModified = objectMetaResponse.LastModified,
                    Security = isPublic ? BlobSecurity.Public : BlobSecurity.Private
                };
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public Stream GetBlobStream(string containerName, string blobName)
        {
            try
            {
                return AsyncHelpers.RunSync(() =>
                    _s3Client.GetObjectStreamAsync(_bucket, GenerateKeyName(containerName, blobName), null));
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
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
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
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

            if (isDownload)
            {
                headers.ContentDisposition = "attachment;";
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                headers.ContentDisposition += "filename=\"" + fileName + "\"";
            }

            if (!string.IsNullOrEmpty(contentType))
            {
                headers.ContentType = contentType;
            }

            var urlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = _bucket,
                Key = GenerateKeyName(containerName, blobName),
                Expires = expiry.UtcDateTime,
                ResponseHeaderOverrides = headers,
                Verb = access == BlobUrlAccess.Read ? HttpVerb.GET : HttpVerb.PUT
            };

            try
            {
                return _s3Client.GetPreSignedURL(urlRequest);
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public IList<BlobDescriptor> ListBlobs(string containerName)
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
                    var objectsResponse = AsyncHelpers.RunSync(() => _s3Client.ListObjectsAsync(objectsRequest));

                    foreach (S3Object entry in objectsResponse.S3Objects)
                    {
                        var objectMetaRequest = new GetObjectMetadataRequest()
                        {
                            BucketName = _bucket,
                            Key = entry.Key
                        };

                        var objectMetaResponse = AsyncHelpers.RunSync(() => _s3Client.GetObjectMetadataAsync(objectMetaRequest));

                        var objectAclRequest = new GetACLRequest()
                        {
                            BucketName = _bucket,
                            Key = entry.Key
                        };

                        var objectAclResponse = AsyncHelpers.RunSync(() => _s3Client.GetACLAsync(objectAclRequest));
                        var isPublic = objectAclResponse.AccessControlList.Grants
                            .Where(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers").Count() > 0;

                        descriptors.Add(new BlobDescriptor
                        {
                            Name = entry.Key.Remove(0, containerName.Length + 1),
                            Container = containerName,
                            Length = entry.Size,
                            ETag = entry.ETag,
                            ContentMD5 = entry.ETag,
                            ContentType = objectMetaResponse.Headers.ContentType,
                            LastModified = entry.LastModified,
                            Security = isPublic ? BlobSecurity.Public : BlobSecurity.Private
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
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
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
                        var objectMetaRequest = new GetObjectMetadataRequest()
                        {
                            BucketName = _bucket,
                            Key = entry.Key
                        };

                        var objectMetaResponse = await _s3Client.GetObjectMetadataAsync(objectMetaRequest);

                        var objectAclRequest = new GetACLRequest()
                        {
                            BucketName = _bucket,
                            Key = entry.Key
                        };

                        var objectAclResponse = await _s3Client.GetACLAsync(objectAclRequest);
                        var isPublic = objectAclResponse.AccessControlList.Grants
                            .Where(x => x.Grantee.URI == "http://acs.amazonaws.com/groups/global/AllUsers").Count() > 0;

                        descriptors.Add(new BlobDescriptor
                        {
                            Name = entry.Key.Remove(0, containerName.Length + 1),
                            Container = containerName,
                            Length = entry.Size,
                            ETag = entry.ETag,
                            ContentMD5 = entry.ETag,
                            ContentType = objectMetaResponse.Headers.ContentType,
                            LastModified = entry.LastModified,
                            Security = isPublic ? BlobSecurity.Public : BlobSecurity.Private
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
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public void SaveBlobStream(string containerName, string blobName, Stream source, BlobProperties properties = null)
        {
            if (source.Length >= 100000000)
            {
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    InputStream = source,
                    PartSize = 6291456,
                    Key = GenerateKeyName(containerName, blobName),
                    ContentType = properties?.ContentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                try
                {
                    AsyncHelpers.RunSync(() => new TransferUtility(_s3Client).UploadAsync(fileTransferUtilityRequest));
                }
                catch (AmazonS3Exception asex)
                {
                    if (IsInvalidAccessException(asex))
                    {
                        throw new StorageException(1000.ToStorageError(), asex);
                    }
                    else
                    {
                        throw new StorageException(1001.ToStorageError(), asex);
                    }
                }

            }
            else
            {
                var putRequest = new PutObjectRequest()
                {
                    BucketName = _bucket,
                    Key = GenerateKeyName(containerName, blobName),
                    InputStream = source,
                    ContentType = properties?.ContentType,
                    CannedACL = GetCannedACL(properties)
                };

                try
                {
                    AsyncHelpers.RunSync(() => _s3Client.PutObjectAsync(putRequest));
                }
                catch (AmazonS3Exception asex)
                {
                    if (IsInvalidAccessException(asex))
                    {
                        throw new StorageException(1000.ToStorageError(), asex);
                    }
                    else
                    {
                        throw new StorageException(1001.ToStorageError(), asex);
                    }
                }
            }
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source, BlobProperties properties = null)
        {
            if (source.Length >= 100000000)
            {
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    InputStream = source,
                    PartSize = 6291456,
                    Key = GenerateKeyName(containerName, blobName),
                    ContentType = properties?.ContentType,
                    CannedACL = GetCannedACL(properties)
                };

                try
                {
                    await new TransferUtility(_s3Client).UploadAsync(fileTransferUtilityRequest);
                }
                catch (AmazonS3Exception asex)
                {
                    if (IsInvalidAccessException(asex))
                    {
                        throw new StorageException(1000.ToStorageError(), asex);
                    }
                    else
                    {
                        throw new StorageException(1001.ToStorageError(), asex);
                    }
                }
            }
            else
            {
                var putRequest = new PutObjectRequest()
                {
                    BucketName = _bucket,
                    Key = GenerateKeyName(containerName, blobName),
                    InputStream = source,
                    ContentType = properties?.ContentType,
                    CannedACL = GetCannedACL(properties)
                };

                try
                {
                    await _s3Client.PutObjectAsync(putRequest);
                }
                catch (AmazonS3Exception asex)
                {
                    if (IsInvalidAccessException(asex))
                    {
                        throw new StorageException(1000.ToStorageError(), asex);
                    }
                    else
                    {
                        throw new StorageException(1001.ToStorageError(), asex);
                    }
                }
            }
        }

        public void UpdateBlobProperties(string containerName, string blobName, BlobProperties properties)
        {
            var updateRequest = new CopyObjectRequest()
            {
                SourceBucket = _bucket,
                SourceKey = GenerateKeyName(containerName, blobName),
                DestinationBucket = _bucket,
                DestinationKey = GenerateKeyName(containerName, blobName),
                ContentType = properties?.ContentType,
                CannedACL = GetCannedACL(properties),
                MetadataDirective = S3MetadataDirective.REPLACE
            };
            updateRequest.Headers.ContentDisposition = properties.ContentDisposition;

            try
            {
                AsyncHelpers.RunSync(() => _s3Client.CopyObjectAsync(updateRequest));
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        public async Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            var updateRequest = new CopyObjectRequest()
            {
                SourceBucket = _bucket,
                SourceKey = GenerateKeyName(containerName, blobName),
                DestinationBucket = _bucket,
                DestinationKey = GenerateKeyName(containerName, blobName),
                ContentType = properties?.ContentType,
                CannedACL = GetCannedACL(properties),
                MetadataDirective = S3MetadataDirective.REPLACE,                
            };
            updateRequest.Headers.ContentDisposition = properties.ContentDisposition;

            try
            {
                await _s3Client.CopyObjectAsync(updateRequest);
            }
            catch (AmazonS3Exception asex)
            {
                if (IsInvalidAccessException(asex))
                {
                    throw new StorageException(1000.ToStorageError(), asex);
                }
                else
                {
                    throw new StorageException(1001.ToStorageError(), asex);
                }
            }
        }

        private S3CannedACL GetCannedACL(BlobProperties properties)
        {
            if (properties?.Security == BlobSecurity.Public)
            {
                return S3CannedACL.PublicRead;
            }
            else
            {
                return S3CannedACL.Private;
            }
        }

        private bool IsInvalidAccessException(AmazonS3Exception asex)
        {
            return asex.ErrorCode != null &&
                        (asex.ErrorCode.Equals("InvalidAccessKeyId") || asex.ErrorCode.Equals("InvalidSecurity"));
        }

        private string GenerateKeyName(string containerName, string blobName)
        {
            return $"{containerName}/{blobName}";
        }
    }
}