using System.IO;

namespace TwentyTwenty.Storage.Amazon.Test
{
    public static class Extensions
    {
        public static Stream AsStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }
    }
}