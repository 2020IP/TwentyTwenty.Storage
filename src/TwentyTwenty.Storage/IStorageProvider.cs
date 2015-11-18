using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TwentyTwenty.Storage
{
    public interface IStorageProvider
    {
        void SaveBlobStream(string containerName, string blobName, Stream source, BlobProperties properties = null);

        Task SaveBlobStreamAsync(string containerName, string blobName, Stream source, BlobProperties properties = null);

        Stream GetBlobStream(string containerName, string blobName);

        Task<Stream> GetBlobStreamAsync(string containerName, string blobName);

        string GetBlobUrl(string containerName, string blobName);

        string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, bool isDownload = false, 
            string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read);

        BlobDescriptor GetBlobDescriptor(string containerName, string blobName);

        Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName);

        IList<BlobDescriptor> ListBlobs(string containerName);

        Task<IList<BlobDescriptor>> ListBlobsAsync(string containerName);

        void DeleteBlob(string containerName, string blobName);

        Task DeleteBlobAsync(string containerName, string blobName);

        void DeleteContainer(string containerName);

        Task DeleteContainerAsync(string containerName);

        void UpdateBlobProperties(string containerName, string blobName, BlobProperties properties);

        Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties);
    }
}