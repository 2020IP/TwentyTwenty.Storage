using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace TwentyTwenty.Storage
{
    public static class Extentions
    {
        public static async Task<string> GetBlobTextAsync(this IStorageProvider provider, string containerName, string blobName)
        {
            using (StreamReader stream = new StreamReader(await provider.GetBlobStreamAsync(containerName, blobName)))
            {
                return await stream.ReadToEndAsync();
            }
        }

        public static async Task<string> GetBlobTextAsync(this IStorageProvider provider, string path)
        {
            return await provider.GetBlobTextAsync(GetContainerName(path), Path.GetFileName(path));
        }

        public static async Task SaveBlobTextAsync(this IStorageProvider provider, string containerName, string blobName, string text)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                await provider.SaveBlobStreamAsync(containerName, blobName, stream);
            }
        }

        public static async Task SaveBlobTextAsync(this IStorageProvider provider, string path, string text )
        {
            await provider.SaveBlobTextAsync(GetContainerName(path), Path.GetFileName(path), text);
        }

        public static async Task CopyFromLocalAsync(this IStorageProvider provider, string localPath, string containerName, string blobName)
        {
            using (System.IO.FileStream stream = File.OpenRead(localPath))
            {
                await provider.SaveBlobStreamAsync(containerName, blobName, stream);
            }
        }

        public static async Task CopyToLocalAsync(this IStorageProvider provider, string containerName, string blobName, string localPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            using (FileStream fileStream = File.Create(localPath))
            {
                using (Stream source = await provider.GetBlobStreamAsync(containerName, blobName))
                { 
                    //S3 doesn't support seek 0 - have to assume at beginning of stream (hashstream)
                    //source.Seek(0, SeekOrigin.Begin);
                    source.CopyTo(fileStream);
                }
            }
        }

        public static async Task CopyContainerToLocalAsync(this IStorageProvider provider, string containerName, string targetFolder)
        {
            IList<BlobDescriptor> contents = await provider.ListBlobsAsync(containerName);

            if (contents != null)
            {
                foreach (BlobDescriptor blob in contents)
                {
                    await provider.CopyToLocalAsync(blob.Path, blob.Path.Replace(containerName, targetFolder));
                }
            }
        }

        public static async Task CopyContainerFromLocalFolderAsync(this IStorageProvider provider, string sourceFolder, string containerName)
        {
            var dirInfo = new DirectoryInfo(sourceFolder);
            var fileInfo = dirInfo.GetFiles("*", SearchOption.AllDirectories);

            foreach (var f in fileInfo)
            {
                await provider.CopyFromLocalAsync(f.FullName, CombineContainerPath(containerName, f.DirectoryName.Substring(sourceFolder.Length)), f.Name);
            }
        }

        public static async Task<string> CopyContainerToLocalTempAsync(this IStorageProvider provider, string containerName)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);

            await provider.CopyContainerToLocalAsync(containerName, tempPath);

            return tempPath;
        }

        public static async Task ProcessLocalContainerActionAsync(this IStorageProvider provider, string containerName, Action<string> action)
        {
            string localPath = await provider.CopyContainerToLocalTempAsync(containerName);

            action(localPath);

            System.IO.Directory.Delete(localPath);
        }

        public static async Task CopyContainerFromLocalActionAsync(this IStorageProvider provider, string containerName, Action<string> action)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);

            action(tempPath);

            await provider.CopyContainerFromLocalFolderAsync(tempPath, containerName);

            System.IO.Directory.Delete(tempPath, true);
        }

        public static async Task<Stream> GetBlobStreamAsync(this IStorageProvider provider, string path)
        {            
            return await provider.GetBlobStreamAsync(GetContainerName(path), Path.GetFileName(path));
        }

        public static async Task CopyToLocalAsync(this IStorageProvider provider, string path, string localPath)
        {
            await provider.CopyToLocalAsync(GetContainerName(path), Path.GetFileName(path), localPath);
        }

        public static async Task CopyFromLocalAsync(this IStorageProvider provider, string localPath, string path)
        {
            await provider.CopyFromLocalAsync(localPath, GetContainerName(path), Path.GetFileName(path));
        }

        public static async Task ProcessLocalActionAsync(this IStorageProvider provider, string path, Action<string> action)
        {
            await ProcessLocalActionAsync(provider, GetContainerName(path), Path.GetFileName(path), action);
        }

        public static async Task CopyFromLocalActionAsync(this IStorageProvider provider, string path, Action<string> action)
        {
            await CopyFromLocalActionAsync(provider, GetContainerName(path), Path.GetFileName(path), action);
        }

        public static async Task UpdateFromLocalActionAsync(this IStorageProvider provider, string path, Action<string> action)
        {
            await UpdateFromLocalActionAsync(provider, GetContainerName(path), Path.GetFileName(path), action);
        }

        public static async Task ProcessLocalActionAsync(this IStorageProvider provider, string containerName, string blobName, Action<string> action)
        {
            string tempFileName = Path.GetTempFileName();

            await provider.CopyToLocalAsync(containerName, blobName, tempFileName);

            action(tempFileName);
            
            System.IO.File.Delete(tempFileName);
        }

        public static async Task CopyFromLocalActionAsync(this IStorageProvider provider, string containerName, string blobName, Action<string> action)
        {
            string tempFileName = Path.GetTempFileName();

            action(tempFileName);

            await provider.CopyFromLocalAsync(tempFileName, containerName, blobName);

            System.IO.File.Delete(tempFileName);
        }

        public static async Task UpdateFromLocalActionAsync(this IStorageProvider provider, string containerName, string blobName, Action<string> action)
        {
            string tempFileName = Path.GetTempFileName();

            await provider.CopyToLocalAsync(containerName, blobName, tempFileName);            

            action(tempFileName);

            await provider.CopyFromLocalAsync(tempFileName, containerName, blobName);

            System.IO.File.Delete(tempFileName);
        }
        public static async Task<string> CopyToLocalTempAsync(this IStorageProvider provider, string containerName, string blobName)
        {
            string tempFileName = Path.GetTempFileName();

            await provider.CopyToLocalAsync(containerName, blobName, tempFileName);

            return tempFileName;
        }

        public static async Task<string> CopyToLocalTempAsync(this IStorageProvider provider, string path)
        {
            return await CopyToLocalTempAsync(provider, GetContainerName(path), Path.GetFileName(path));
        }

        public static async Task DeleteBlobAsync(this IStorageProvider provider, string path)
        {
            await provider.DeleteBlobAsync(GetContainerName(path), Path.GetFileName(path));
        }

        public static async Task CopyBlobAsync(this IStorageProvider provider, string sourcePath, string targetPath)
        {
            await provider.CopyBlobAsync(GetContainerName(sourcePath), Path.GetFileName(sourcePath), GetContainerName(targetPath), Path.GetFileName(targetPath));
        }

        public static async Task<BlobDescriptor> GetBlobDescriptorAsync(this IStorageProvider provider, string path)
        {
            return await provider.GetBlobDescriptorAsync(GetContainerName(path), Path.GetFileName(path));
        }

        public static async Task<IList<BlobDescriptor>> ListBlobsAsync(this IStorageProvider provider, string containerName, string filterRegex)
        {
            IList<BlobDescriptor> list = await provider.ListBlobsAsync(containerName);

            Regex regex = new Regex(filterRegex, RegexOptions.IgnoreCase);

            return list.Where(i => regex.IsMatch(i.Name)).ToList();
        }

        public static async Task CopyContainerAsync(this IStorageProvider provider, string sourceContainer, string targetContainer)
        {
            IList<BlobDescriptor> list = await provider.ListBlobsAsync(sourceContainer);

           await Task.WhenAll(list.Select(async (blob) =>
            {
                await provider.CopyBlobAsync(blob.Path, blob.Path.Replace(sourceContainer, targetContainer));
            }));
        }

        //public static string GetFullName(this BlobDescriptor blobDescriptor)
        //{
        //    return blobDescriptor.Container + blobDescriptor.Name;
        //}

        private static string GetContainerName(string path)
        {
            return Path.GetDirectoryName(path).Replace('\\', '/');
        }

        private static string CombineContainerPath(params string[] paths)
        {
            return Path.Combine(paths.Select(i => i.Replace('\\','/').TrimStart('/')).ToArray()).Replace('\\', '/');
        }

        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (dateTimeOffset.ToUniversalTime() - unixStart).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        public static StorageError ToStorageError(this int code)
            => ((StorageErrorCode) code).ToStorageError();

        public static StorageError ToStorageError(this StorageErrorCode code)
        {
            var error = Errors
                .Where(x => x.Key == code)
                .Select(x => new StorageError() {Code = (int) x.Key, Message = x.Value})
                .FirstOrDefault();

            return error ?? new StorageError
                   {
                       Code = (int) StorageErrorCode.GenericException,
                       Message = "Generic provider exception occurred",
                   };
        }

        public static List<T2> SelectToListOrEmpty<T1, T2>(this IEnumerable<T1> e, Func<T1, T2> f)
            => e == null ? new List<T2>() : e.Select(f).ToList();

        public static List<T1> WhereToListOrEmpty<T1>(this IEnumerable<T1> e, Func<T1, bool> f)
            => e == null ? new List<T1>() : e.Where(f).ToList();

        private static readonly Dictionary<StorageErrorCode, string> Errors = new Dictionary<StorageErrorCode, string>
        {
            {
                StorageErrorCode.InvalidCredentials,
                "Invalid security credentials"
            },
            {
                StorageErrorCode.GenericException,
                "Generic provider exception occurred"
            },
            {
                StorageErrorCode.InvalidAccess,
                "Invalid access permissions."
            },
            {
                StorageErrorCode.BlobInUse,
                "Blob in use."
            },
            {
                StorageErrorCode.InvalidName,
                "Invalid blob or container name."
            },
            {
                StorageErrorCode.ErrorOpeningBlob,
                "Error opening blob."
            },
            {
                StorageErrorCode.NoCredentialsProvided,
                "No credentials provided."
            },
            {
                StorageErrorCode.NotFound,
                "Blob or container not found."
            }
        };
    }
}