using Google.Apis.Storage.v1;

namespace TwentyTwenty.Storage.Google
{
    public class GoogleProviderOptions
    {
        public string Email { get; set; }

        public string PrivateKey { get; set; }

        public string Bucket { get; set; }
    }
}