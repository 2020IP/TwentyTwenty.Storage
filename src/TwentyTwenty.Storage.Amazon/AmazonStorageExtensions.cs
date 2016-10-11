using System.Collections.Generic;
using System.Linq;
using Amazon.S3.Model;

namespace TwentyTwenty.Storage.Amazon
{
    public static class AmazonStorageExtensions
    {
        public static IDictionary<string, string> ToMetadata(this MetadataCollection amzMeta)
        {
            return amzMeta.Keys.ToDictionary(k => k.Replace("x-amz-meta-", string.Empty), k => amzMeta[k]);
        }

        public static void AddMetadata(this MetadataCollection amzMeta, IDictionary<string, string> meta)
        {
            if (meta != null)
            {
                foreach (var kvp in meta)
                {
                    amzMeta[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}