using System;
using System.IO;

namespace TwentyTwenty.Storage.Local.Test
{
    public class StorageFixture : IDisposable
    {
        public const string BasePath = "C:\\temp";
        public const string ContainerPrefix = "storagetest-";

        public StorageFixture()
        {
        }

        public void Dispose()
        {
            foreach (var d in Directory.GetDirectories(BasePath))
            {
                if (d.Replace($"{BasePath}\\", "").StartsWith(ContainerPrefix))
                {
                    Directory.Delete(d, true);
                }
            }
        }
    }
}