using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TwentyTwenty.Storage.Local.Test
{
    [Trait("Category", "Local")]
    public sealed class UpdateBlobPropertiesTests : BlobTestBase
    {
        public UpdateBlobPropertiesTests()
            : base() { }

        [Fact]
        public async void Test_Updating_Properties_Stores_Meta_File()
        {
            var destinationContainer = GetRandomContainerName();
            var destinationName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(destinationContainer, destinationName, data);

            var propertiesToStore = new BlobProperties
            {
                Security = BlobSecurity.Public,
                ContentType = "application/octet-stream",
                ContentDisposition = "inline",
                Metadata = new Dictionary<string, string>
                {
                    {"custom", "data"},
                }
            };
            _provider.UpdateBlobProperties(destinationContainer, destinationName, propertiesToStore);
            
            // Make sure that update blob properties stored the meta file
            var metaFileName = Path.Combine(BasePath, destinationContainer, $"{destinationName}-meta.json");
            await using var file = File.OpenRead(metaFileName);
            using var streamReader = new StreamReader(file);
            var fileContent = await streamReader.ReadToEndAsync();
            var parsedBlobProperties = JsonSerializer.Deserialize<BlobProperties>(fileContent, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter()}
            });
            
            Assert.NotNull(parsedBlobProperties);
            Assert.Equal(propertiesToStore.Security, parsedBlobProperties.Security);
            Assert.Equal(propertiesToStore.ContentType, parsedBlobProperties.ContentType);
            Assert.Equal(propertiesToStore.ContentDisposition, parsedBlobProperties.ContentDisposition);
            Assert.Equal(propertiesToStore.Metadata.Count, parsedBlobProperties.Metadata.Count);
        }
        
        [Fact]
        public void Test_Updating_Properties_Returns_It_On_Read()
        {
            var destinationContainer = GetRandomContainerName();
            var destinationName = GenerateRandomName();
            var data = GenerateRandomBlobStream();

            CreateNewFile(destinationContainer, destinationName, data);

            var propertiesToStore = new BlobProperties
            {
                Security = BlobSecurity.Public,
                ContentType = "application/octet-stream",
                ContentDisposition = "inline",
                Metadata = new Dictionary<string, string>
                {
                    {"custom", "data"},
                }
            };
            _provider.UpdateBlobProperties(destinationContainer, destinationName, propertiesToStore);

            var blob = _provider.GetBlobDescriptor(destinationContainer, destinationName);
            
            Assert.Equal(propertiesToStore.Security, blob.Security);
            Assert.Equal(propertiesToStore.ContentDisposition, blob.ContentDisposition);
            Assert.Equal(propertiesToStore.ContentType, blob.ContentType);
            Assert.Equal(propertiesToStore.Metadata, blob.Metadata);
        }
    }
}