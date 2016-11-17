using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [CollectionDefinition("BlobTestBase")]
    public class BaseCollection : ICollectionFixture<StorageFixture>
    {
    }

    [Collection("BlobTestBase")]
    public abstract class BlobTestBase
    {
        Random _rand = new Random();
        protected LocalStorageProvider _provider;
        protected string BasePath;

        public BlobTestBase(StorageFixture fixture)
        {
            BasePath = StorageFixture.BasePath;
            _provider = new LocalStorageProvider(StorageFixture.BasePath);
        }

        protected byte[] GenerateRandomBlob(int length = 256)
        {
            var buffer = new Byte[length];
            _rand.NextBytes(buffer);
            return buffer;
        }

        protected MemoryStream GenerateRandomBlobStream(int length = 256)
        {
            return new MemoryStream(GenerateRandomBlob(length));
        }

        protected string GetRandomContainerName()
        {
            return StorageFixture.ContainerPrefix + Guid.NewGuid().ToString("N");
        }

        protected string GenerateRandomName()
        {
            return Guid.NewGuid().ToString("N");
        }

        protected void CreateNewFile(string containerName, string blobName, Stream source)
        {
            var dir = Path.Combine(BasePath, containerName);
            Directory.CreateDirectory(dir);
            using (var file = File.Create(Path.Combine(dir, blobName)))
            {
                source.CopyTo(file);
            }
        }

        protected bool StreamEquals(Stream stream1, Stream stream2)
        {
            if (stream1.CanSeek)
            {
                stream1.Seek(0, SeekOrigin.Begin);
            }
            if (stream2.CanSeek)
            {
                stream2.Seek(0, SeekOrigin.Begin);
            }

            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize]; //buffer size
            byte[] buffer2 = new byte[bufferSize];
            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                // You might replace the following with an efficient "memcmp"
                if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
                    return false;
            }
        }
    }
}