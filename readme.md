#20|20 Storage

[![Build status](https://ci.appveyor.com/api/projects/status/0ss5kpj5gy739vwx/branch/master?svg=true)](https://ci.appveyor.com/project/2020IP/twentytwenty-storage/branch/master)
[![Nuget Version](https://img.shields.io/nuget/v/TwentyTwenty.Storage.svg)](https://www.nuget.org/packages/TwentyTwenty.Storage/)

<!--TravisCI: [![Build Status](https://travis-ci.org/2020IP/TwentyTwenty.Storage.svg)](https://travis-ci.org/2020IP/TwentyTwenty.Storage)-->

### Basic Usage

Initialization:
```
IStorageProvider provider = new AmazonStorageProvider(new AmazonProviderOptions
{
    Bucket = "mybucketname",
    PublicKey = "mypublickey",
    SecretKey = "mysecretkey"
});
```

### Installing User Secrets Manager
The secret manager can be installed globally with:
```
dnu commands install Microsoft.Extensions.SecretManager
```

### Adding Project-Specific Secrets
The functional tests depend on access to actual cloud storage provider accounts.  Providing these configuration values can be done through environment variables or the user secret store. Here is how to add settings:

Azure:
```
cd test\TwentyTwenty.Storage.Azure.Test
user-secret set ConnectionString "my_really_long_connection_string"
```
Amazon:
```
cd test\TwentyTwenty.Storage.Amazon.Test
user-secret set PublicKey "my_public_key"
user-secret set PrivateKey "my_private_key"
user-secret set Bucket "my_bucket_name"
```
Google:
```
cd test\TwentyTwenty.Storage.Google.Test
user-secret set GoogleEmail "my_google_storage_api_email"
user-secret set GoogleBucket "my_google_bucket"
user-secret set GoogleP12PrivateKey "my_base64_encoded_byte_array_google_p12_key"

NOTE: To get the 'GoogleP12PrivateKey' get the byte array from the P12 certificate for your Google Cloud Storage API and base64 encode it
```
Secrets can be listed with:
```
cd test\TwentyTwenty.Storage.Azure.Test
user-secret list
```
