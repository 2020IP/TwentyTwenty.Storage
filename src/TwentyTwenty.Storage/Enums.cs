using System;

namespace TwentyTwenty.Storage
{
    public enum BlobSecurity
    {
        Private,
        Public
    }

    [Flags]
    public enum BlobUrlAccess
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        All = Read | Write | Delete,
    }

    public enum StorageErrorCode
    {
        None = 0,
        InvalidCredentials = 1000,
        GenericException = 1001,
        InvalidAccess = 1002,
        BlobInUse = 1003,
        InvalidBlobName = 1004,
        InvalidContainerName = 1005,
        ErrorOpeningBlob = 1006
    }
}