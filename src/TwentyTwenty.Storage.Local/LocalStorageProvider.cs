using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwentyTwenty.Storage.Local
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _basePath;
        private readonly JsonSerializerOptions _jsonSerialiserOptions;

        public LocalStorageProvider(string basePath)
        {
            _basePath = basePath;
            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = true
            };
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            var path = Path.Combine(_basePath, containerName, blobName);

            if (!File.Exists(path))
            {
                throw new StorageException(StorageErrorCode.InvalidName.ToStorageError(), null);
            }

            try
            {
                File.Delete(path);
                var metaPath = CreateMetadataPath(path);
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
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
                var path = Path.Combine(_basePath, containerName);
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
            }
        }

        public Task DeleteContainerAsync(string containerName)
        {
            return Task.Run(() => DeleteContainer(containerName));
        }

        public Task CopyBlobAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName,
            string destinationBlobName = null)
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

            var sourcePath = Path.Combine(_basePath, sourceContainerName, sourceBlobName);
            var destPath = Path.Combine(_basePath, destinationContainerName, destinationBlobName ?? sourceBlobName);

            var destDir = Path.GetDirectoryName(destPath);
            Directory.CreateDirectory(destDir);

            File.Copy(sourcePath, destPath, true);

            var sourceMetaPath = CreateMetadataPath(sourcePath);
            if (File.Exists(sourceMetaPath))
            {
                var destMetaPath = CreateMetadataPath(destPath);
                File.Copy(sourceMetaPath, destMetaPath, true);
            }
            return Task.FromResult(true);
        }

        public async Task MoveBlobAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName,
            string destinationBlobName = null)
        {
            await CopyBlobAsync(sourceContainerName, sourceBlobName, destinationContainerName, destinationBlobName);
            await DeleteBlobAsync(sourceContainerName, sourceBlobName);
        }

        public BlobDescriptor GetBlobDescriptor(string containerName, string blobName)
        {
            var path = Path.Combine(_basePath, containerName, blobName);

            try
            {
                var info = new FileInfo(path);

                var descriptor = new BlobDescriptor
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
                MergeDescriptorWithCustomMetaIfExists(info.FullName, descriptor);

                return descriptor;
            }
            catch (Exception ex)
            {
                throw new StorageException(1001.ToStorageError(), ex);
            }
        }

        public Task<BlobDescriptor> GetBlobDescriptorAsync(string containerName, string blobName)
        {
            return Task.Run(() => GetBlobDescriptor(containerName, blobName));
        }

        public Task<bool> DoesBlobExistAsync(string containerName, string blobName)
        {
            return Task.Run(() =>
            {
                var path = Path.Combine(_basePath, containerName, blobName);
                return File.Exists(path);
            });
        }

        public string GetBlobSasUrl(string containerName, string blobName, DateTimeOffset expiry,
            bool isDownload = false, string fileName = null, string contentType = null, BlobUrlAccess access = BlobUrlAccess.Read)
        {
            return Path.Combine(_basePath, containerName, blobName);
        }

        public Stream GetBlobStream(string containerName, string blobName)
        {
            try
            {
                var path = Path.Combine(_basePath, containerName, blobName);
                return File.OpenRead(path);
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            return await Task.Run(() => GetBlobStream(containerName, blobName));
        }

        public string GetBlobUrl(string containerName, string blobName)
        {
            return Path.Combine(_basePath, containerName, blobName);
        }

        public IList<BlobDescriptor> ListBlobs(string containerName)
        {
            var localFilesInfo = new List<BlobDescriptor>();

            try
            {
                var dir = Path.Combine(_basePath, containerName);
                var dirInfo = new DirectoryInfo(dir);
                var fileInfo = dirInfo.GetFiles("*", SearchOption.AllDirectories);

                foreach (var f in fileInfo)
                {
                    var blobDescriptor = new BlobDescriptor
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
                    };
                    MergeDescriptorWithCustomMetaIfExists(f.FullName, blobDescriptor);
                    localFilesInfo.Add(blobDescriptor);
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
            return Task.Run(() => ListBlobs(containerName));
        }

        public void SaveBlobStream(string containerName, string blobName, Stream source,
            BlobProperties properties = null, bool closeStream = true)
        {
            var path = ExtractFullPathAndProtectAgainstPathTraversal(containerName, blobName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var file = File.Create(path))
                {
                    source.CopyTo(file);
                }

                if (closeStream)
                {
                    source.Dispose();
                }
                if (properties != default)
                {
                    UpdateBlobProperties(containerName, blobName, properties);
                }
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
            }
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source,
            BlobProperties properties = null, bool closeStream = true, long? length = null)
        {
            var path = ExtractFullPathAndProtectAgainstPathTraversal(containerName, blobName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var file = File.Create(path))
                {
                    await source.CopyToAsync(file);
                }

                if (closeStream)
                {
                    source.Dispose();
                }
                if (properties != default)
                {
                    await UpdateBlobPropertiesAsync(containerName, blobName, properties);
                }
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
            }
        }

        public void UpdateBlobProperties(string containerName, string blobName, BlobProperties properties)
        {
            try
            {
                var path = ExtractFullPathAndProtectAgainstPathTraversal(containerName, blobName);
                var metaPath = CreateMetadataPath(path);

                var json = JsonSerializer.Serialize(properties, _jsonSerialiserOptions);

                using (var file = File.Create(metaPath))
                using (var streamWriter = new StreamWriter(file))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
            }
        }

        public async Task UpdateBlobPropertiesAsync(string containerName, string blobName, BlobProperties properties)
        {
            try
            {
                var path = ExtractFullPathAndProtectAgainstPathTraversal(containerName, blobName);
                var metaPath = CreateMetadataPath(path);

                var json = JsonSerializer.Serialize(properties, _jsonSerialiserOptions);

                using (var file = File.Create(metaPath))
                using (var streamWriter = new StreamWriter(file))
                {
                    await streamWriter.WriteAsync(json);
                    await streamWriter.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex.ToStorageException();
            }
        }

        private string ExtractFullPathAndProtectAgainstPathTraversal(string containerName, string blobName)
        {
            var dir = Path.Combine(_basePath, containerName);
            var path = Path.Combine(dir, blobName);

            if (!Path.GetFullPath(path).StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Detected path traversal attempt.").ToStorageException();
            }

            return path;
        }


        private static string CreateMetadataPath(string path)
        {
            return $"{path}-meta.json";
        }

        private void MergeDescriptorWithCustomMetaIfExists(string fullPathToBlob, BlobDescriptor descriptor)
        {
            var meta = GetFileMetaIfExists(fullPathToBlob);
            if (meta != default)
            {
                if (!string.IsNullOrWhiteSpace(meta.ContentDisposition))
                {
                    descriptor.ContentDisposition = meta.ContentDisposition;
                }

                if (!string.IsNullOrWhiteSpace(meta.ContentType))
                {
                    descriptor.ContentType = meta.ContentType;
                }

                descriptor.Security = meta.Security;
                descriptor.Metadata = meta.Metadata;
            }
        }

        private BlobDescriptor GetFileMetaIfExists(string fullPathToBlob)
        {
            var metaPath = CreateMetadataPath(fullPathToBlob);

            if (!File.Exists(metaPath)) return null;

            using (var file = File.OpenRead(metaPath))
            using (var streamReader = new StreamReader(file))
            {
                var metaContent = streamReader.ReadToEnd();
                return JsonSerializer.Deserialize<BlobDescriptor>(metaContent, _jsonSerialiserOptions);
            }
        }

        private async Task<BlobDescriptor> GetFileMetaIfExistsAsync(string fullPathToBlob)
        {
            var metaPath = CreateMetadataPath(fullPathToBlob);

            if (!File.Exists(metaPath)) return null;

            using (var file = File.OpenRead(metaPath))
            using (var streamReader = new StreamReader(file))
            {
                var metaContent = await streamReader.ReadToEndAsync();
                return JsonSerializer.Deserialize<BlobDescriptor>(metaContent, _jsonSerialiserOptions);
            }
        }
    }
}