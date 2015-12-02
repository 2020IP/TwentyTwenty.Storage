using System.IO;

namespace TwentyTwenty.Storage.Google.Test
{
    public static class Extensions
    {
        public static Stream AsStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }
    }
}