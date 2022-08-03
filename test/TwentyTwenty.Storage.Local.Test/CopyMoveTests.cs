using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class CopyMoveTests : BlobTestBase
    {
        public CopyMoveTests(StorageFixture fixture)
            : base() { }

        [Fact]
        public async void Test_Blob_Moved()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
        }
        
        [Fact]
        public async void Test_Blob_Moved_With_Metadata()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.UpdateBlobPropertiesAsync(sourceContainer, sourceName, new BlobProperties
            {
                Security = BlobSecurity.Public
            });
            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
            
            // Make sure that destination -meta.json file exists
            Assert.True(File.Exists(Path.Combine(BasePath, destContainer, $"{sourceName}-meta.json")));

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
            
            // Maure sure source -meta.json file no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, $"{sourceName}-meta.json")));
        }

        [Fact]
        public async void Test_Blob_Moved_RelativePath()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = "somePatch/test.txt";
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
        }

        [Fact]
        public async void Test_Blob_Moved_And_Renamed()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
        }
        
        [Fact]
        public async void Test_Blob_Moved_And_Renamed_With_Metadata()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);
            await _provider.UpdateBlobPropertiesAsync(sourceContainer, sourceName, new BlobProperties
            {
                Security = BlobSecurity.Public
            });
            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure destination -meta.json now exists 
            Assert.True(File.Exists(Path.Combine(BasePath, destContainer, $"{destName}-meta.json")));
            
            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
            
            // Make sure source -meta.json no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, $"{sourceName}-meta.json")));
        }

        [Fact]
        public async void Test_Blob_Moved_And_Renamed_RelativePath()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = "somePatch/test.txt";
            var destContainer = GetRandomContainerName();
            var destName = "somePatch2/test2.txt";
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.MoveBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source no longer exists
            Assert.False(File.Exists(Path.Combine(BasePath, sourceContainer, sourceName)));
        }

        [Fact]
        public async void Test_Blob_Copied()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer);

            data.ToString();
            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        } 
        
        [Fact]
        public async void Test_Blob_Copied_With_Metadata()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.UpdateBlobPropertiesAsync(sourceContainer, sourceName, new BlobProperties
            {
                Security = BlobSecurity.Public
            });
            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer);

            data.ToString();
            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
            
            // Make sure destination -meta.json now exists 
            Assert.True(File.Exists(Path.Combine(BasePath, destContainer, $"{sourceName}-meta.json")));
            
            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
            // Make sure source -meta.json still exists 
            Assert.True(File.Exists(Path.Combine(BasePath, sourceContainer, $"{sourceName}-meta.json")));
        }

        [Fact]
        public async void Test_Blob_Copied_RelativePath()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = "somePatch/test.txt";
            var destContainer = GetRandomContainerName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer);

            data.ToString();
            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        }

        [Fact]
        public async void Test_Blob_Copied_With_New_Name()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }

            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
        }
        
        [Fact]
        public async void Test_Blob_Copied_With_New_Name_With_Meta()
        {
            var sourceContainer = GetRandomContainerName();
            var sourceName = GenerateRandomName();
            var destContainer = GetRandomContainerName();
            var destName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(sourceContainer, sourceName, data);

            await _provider.UpdateBlobPropertiesAsync(sourceContainer, sourceName, new BlobProperties
            {
                Security = BlobSecurity.Public
            });
            await _provider.CopyBlobAsync(sourceContainer, sourceName, destContainer, destName);

            // Make sure destination now exists and contains original data.
            using (var file = File.OpenRead(Path.Combine(BasePath, destContainer, destName)))
            {
                Assert.True(StreamEquals(data, file));
            }
            // Make sure destination -meta.json now exists
            Assert.True(File.Exists(Path.Combine(BasePath, destContainer, $"{destName}-meta.json")));
            
            // Make sure source still exists
            using (var file = File.OpenRead(Path.Combine(BasePath, sourceContainer, sourceName)))
            {
                Assert.True(StreamEquals(data, file));
            }
            // Make sure source -meta.json now exists
            Assert.True(File.Exists(Path.Combine(BasePath, sourceContainer, $"{sourceName}-meta.json")));
        }
    }
}