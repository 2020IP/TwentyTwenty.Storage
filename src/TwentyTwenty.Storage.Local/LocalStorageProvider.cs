using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TwentyTwenty.Storage.Local
{
    public class LocalStorageProvider : IStorageProvider
    {
        readonly string _basePath;

        public LocalStorageProvider(string basePath)
        {
            _basePath = basePath;
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            try
            {
                File.Delete($"{_basePath}\\{containerName}\\{blobName}");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new StorageException(1002.ToStorageError(), ex);
            }
            catch (ArgumentException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new StorageException(1005.ToStorageError(), ex);
            }
            catch (NotSupportedException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (IOException ex)
            {
                throw new StorageException(1003.ToStorageError(), ex);
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public Task DeleteBlobAsync(string containerName, string blobName)
        {
            return Task.Factory.StartNew(() => DeleteBlob(containerName, blobName));
        }

        public void DeleteContainer(string containerName)
        {
            try
            {
                Directory.Delete($"{_basePath}\\{containerName}", true);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new StorageException(1002.ToStorageError(), ex);
            }
            catch (ArgumentException ex)
            {
                throw new StorageException(1005.ToStorageError(), ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new StorageException(1005.ToStorageError(), ex);
            }
            catch (IOException ex)
            {
                throw new StorageException(1003.ToStorageError(), ex);
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public Task DeleteContainerAsync(string containerName)
        {
            return Task.Factory.StartNew(() => DeleteContainer(containerName));
        }

        public BlobDescriptor GetBlobDescriptor(string containerName, string blobName)
        {
            var path = $"{_basePath}\\{containerName}\\{blobName}";

            try
            {
                var info = new FileInfo(path);

                return new BlobDescriptor
                {
                    Container = containerName,
                    ContentMD5 = "",
                    ContentType = info.Extension.GetMimeType(),
                    ETag = "",
                    LastModified = info.LastWriteTimeUtc,
                    Length = info.Length,
                    Name = info.Name,
                    Security = BlobSecurity.Private,
                    Url = info.FullName
                };
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            return Task.Factory.StartNew(() => GetBlobDescriptor(containerName, blobName));
        }

        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry, 
            bool isDownload = false, string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            return $"{_basePath}\\{containerName}\\{blobName}";
        }

        public Stream GetBlobStream(string containerName, string blobName)
        {
            try
            {
                return File.OpenRead($"{_basePath}\\{containerName}\\{blobName}");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new StorageException(1002.ToStorageError(), ex);
            }
            catch (ArgumentException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new StorageException(1005.ToStorageError(), ex);
            }
            catch (NotSupportedException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (FileNotFoundException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (IOException ex)
            {
                throw new StorageException(1006.ToStorageError(), ex);
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            return Task.Factory.StartNew(() => GetBlobStream(containerName, blobName));
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            return $"{_basePath}\\{containerName}\\{blobName}";
        }

        public IList<BlobDescriptor> ListBlobs(string containerName)
        {
            var localFilesInfo = new List<BlobDescriptor>();

            try
            {
                var dirInfo = new DirectoryInfo($"{_basePath}\\{containerName}");
                var fileInfo = dirInfo.GetFiles();

                foreach (var f in fileInfo)
                {
                    localFilesInfo.Add(new BlobDescriptor
                    {
                        ContentMD5 = "",
                        ETag = "",
                        ContentType = f.Extension.GetMimeType(),
                        Container = containerName,
                        LastModified = f.LastWriteTime,
                        Length = f.Length,
                        Name = f.Name,
                        Url = f.FullName,
                        Security = BlobSecurity.Private,
                    });
                }

                return localFilesInfo;
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public Task<IList<BlobDescriptor>> ListBlobsAsync(string containerName)
        {
            return Task.Factory.StartNew(()=> ListBlobs(containerName));
        }

        public void SaveBlobStream(string containerName, string blobName, Stream source, BlobProperties properties = null)
        {
            var dir = $"{_basePath}\\{containerName}";

            try
            {
                Directory.CreateDirectory(dir);
                using (var file = File.Create($"{dir}\\{blobName}"))
                {
                    source.CopyTo(file);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new StorageException(1002.ToStorageError(), ex);
            }
            catch (ArgumentException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new StorageException(1005.ToStorageError(), ex);
            }
            catch (NotSupportedException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (IOException ex)
            {
                throw new StorageException(1006.ToStorageError(), ex);
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source, BlobProperties properties = null)
        {
            var dir = $"{_basePath}\\{containerName}";

            try
            {
                Directory.CreateDirectory(dir);
                using (var file = File.Create($"{dir}\\{blobName}"))
                {
                    await source.CopyToAsync(file);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new StorageException(1002.ToStorageError(), ex);
            }
            catch (ArgumentException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new StorageException(1005.ToStorageError(), ex);
            }
            catch (NotSupportedException ex)
            {
                throw new StorageException(1004.ToStorageError(), ex);
            }
            catch (IOException ex)
            {
                throw new StorageException(1006.ToStorageError(), ex);
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public void UpdateBlobProperties(string containerName, string blobName, BlobProperties properties)
        {
            return;
        }

        public Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            return Task.FromResult(0);
        }
    }
}