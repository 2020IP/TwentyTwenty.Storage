using System;
using System.IO;

namespace TwentyTwenty.Storage.Local.Test
{
    public sealed class StorageFixture : IDisposable
    {
        public static readonly string BasePath = Path.GetTempPath();
        public const string ContainerPrefix = "storagetest-";

        public StorageFixture()
        {
        }

        public void Dispose()
        {
            var baseDir = new DirectoryInfo(BasePath);

            foreach (var d in baseDir.GetDirectories($"{ContainerPrefix}*"))
            {
                d.Delete(true);
            }
        }
    }
}