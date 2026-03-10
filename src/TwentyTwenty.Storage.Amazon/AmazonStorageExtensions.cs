using System.Collections.Generic;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;

namespace TwentyTwenty.Storage.Amazon
{
    public static class AmazonStorageExtensions
    {
        public static IDictionary<string, string> ToMetadata(this MetadataCollection amzMeta)
        {
            return amzMeta.Keys.ToDictionary(k => k.Replace("x-amz-meta-", string.Empty), k => amzMeta[k]);
        }

        public static void AddMetadata(this MetadataCollection amzMeta, IDictionary<string, string> meta)
        {
            if (meta == null)
            {
                return;
            }

            foreach (var kvp in meta)
            {
                amzMeta[kvp.Key] = kvp.Value;
            }
        }

        public static StorageException ToStorageException(this AmazonS3Exception asex)
        {
            // AWSSDK.S3 v4 uses typed exceptions (e.g. NoSuchKeyException) which may have an empty ErrorCode.
            // Check by type first, then fall back to ErrorCode string matching.
            if (asex is NoSuchKeyException || asex is NoSuchBucketException || asex is NoSuchUploadException)
            {
                return new StorageException(StorageErrorCode.NotFound, asex);
            }

            switch (asex?.ErrorCode)
            {
                case "InvalidAccessKeyId":
                case "Forbidden":
                    return new StorageException(StorageErrorCode.InvalidCredentials, asex);
                case "InvalidSecurity":
                case "AccessDenied":
                case "AccountProblem":
                case "NotSignedUp":
                case "InvalidPayer":
                case "RequestTimeTooSkewed":
                case "SignatureDoesNotMatch":
                    return new StorageException(StorageErrorCode.InvalidAccess, asex);
                case "NoSuchBucket":
                case "NoSuchKey":
                case "NoSuchUpload":
                case "NoSuchVersion":
                    return new StorageException(StorageErrorCode.NotFound, asex);
                case "InvalidBucketName":
                case "KeyTooLong":
                    return new StorageException(StorageErrorCode.InvalidName, asex);
                default:
                    return new StorageException(StorageErrorCode.GenericException, asex);
            }
        }
    }
}