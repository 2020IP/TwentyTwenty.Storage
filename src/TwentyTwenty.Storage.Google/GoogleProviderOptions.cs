using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Storage.v1;

namespace TwentyTwenty.Storage.Google
{
    public class GoogleProviderOptions
    {
        public StorageService GoogleStorageClient { get; set; }

        public string Bucket { get; set; }
    }
}
