using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Azure;
using Azure.Storage.Sas;

namespace TwentyTwenty.Storage.Azure
{
    public static class Extensions
    {
        public static BlobSasPermissions? ToPermissions(this BlobUrlAccess security)
        {
            switch (security)
            {
                case BlobUrlAccess.Read:
                    return BlobSasPermissions.Read;
                case BlobUrlAccess.Write:
                    return BlobSasPermissions.Read | BlobSasPermissions.Write;
                case BlobUrlAccess.Delete:
                    return BlobSasPermissions.Delete;
                case BlobUrlAccess.All:
                    return BlobSasPermissions.All;
            }
            return null;
        }

        public static Exception Convert(this Exception e)
        {
            var storageException = (e as RequestFailedException) ?? e.InnerException as RequestFailedException;

            if (storageException != null)
            {
                StorageErrorCode errorCode;

                switch ((HttpStatusCode)storageException.Status)
                {
                    case HttpStatusCode.Forbidden:
                        errorCode = StorageErrorCode.InvalidCredentials;
                        break;
                    case HttpStatusCode.NotFound:
                        errorCode = StorageErrorCode.InvalidName;
                        break;
                    default:
                        errorCode = StorageErrorCode.GenericException;
                        break;
                }

                return new StorageException(errorCode.ToStorageError(), storageException);
            }
            return e;
        }

        public static bool IsAzureStorageException(this Exception e)
        {
            return e is RequestFailedException || e.InnerException is RequestFailedException;
        }

        public static void SetMetadata(this IDictionary<string, string> azureMeta, IDictionary<string, string> meta)
        {
            azureMeta.Clear();

            if (meta != null)
            {
                foreach (var kvp in meta)
                {
                    azureMeta[kvp.Key] = kvp.Value;
                }
            }
        }

        public static string ToHex(this byte[] value) => string.Join(string.Empty, value.Select(x => x.ToString("X2")));
    }
}