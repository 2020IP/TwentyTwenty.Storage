# 20|20 Storage

[![Build status](https://ci.appveyor.com/api/projects/status/0ss5kpj5gy739vwx/branch/master?svg=true)](https://ci.appveyor.com/project/2020IP/twentytwenty-storage/branch/master)
[![Nuget Version](https://img.shields.io/nuget/v/TwentyTwenty.Storage.svg)](https://www.nuget.org/packages/TwentyTwenty.Storage/)

## Overview

20|20 Storage uses the least common denominator of functionality between the supported providers to build a cross-cloud storage solution.

Currently supported providers are:
* Azure Blob Storage
* Amazon S3
* Google Cloud Storage
* Local File System Storage (Signed URL and Update Properties not implemented)

#### Featured on .NET Rocks! [Show 1277](https://www.dotnetrocks.com/?show=1277)

## Basic Usage

#### Initialization:
```
IStorageProvider provider = new AmazonStorageProvider(new AmazonProviderOptions
{
    Bucket = "mybucketname",
    PublicKey = "mypublickey",
    SecretKey = "mysecretkey"
});
```
(See next section for adding provider accounts)

##### Saving a blob:
```
// Defualt blob properties can also be passed as an additional parameter
await _provider.SaveBlobStreamAsync(containerName, blobName, dataStream);
```

##### Updating a blobs properties:
```
await _provider.UpdateBlobPropertiesAsync(containerName, blobName, new BlobProperties
{
    ContentType = "application/json",
    Security = BlobSecurity.Public
});
```

##### Listing a containers blobs:
```
foreach (var blob in await _provider.ListBlobsAsync(containerName))
{
    // Do something with the blobs
}
```

##### Getting a blobs descriptor:
```
var descriptor = await _provider.GetBlobDescriptorAsync(containerName, blobName);
```

##### Getting a blobs stream:
```
using (var blobStream = await _provider.GetBlobStreamAsync(containerName, blobName))
{
    // Do something with the stream
}
```

##### Getting a blobs url:
```
var url = _provider.GetBlobUrl(containerName, blobName);
```

##### Getting a blobs signed(sas) read url:
```
var readUrl = _provider.GetBlobSasUrl(containerName, blobName, DateTimeOffset.UtcNow.AddMinutes(5));
```

##### Getting a blobs signed(sas) downloadable url:
```
var url = _provider.GetBlobSasUrl(containerName, blobName, DateTimeOffset.UtcNow.AddMinutes(5), isDownload: true, "myfilename.txt", "text/plain");
```

##### Getting a blobs signed(sas) write url and writing to it:
```
var writeUrl = _provider.GetBlobSasUrl(containerName, blobName, DateTimeOffset.UtcNow.AddMinutes(5), access: BlobUrlAccess.Write);

var httpRequest = WebRequest.Create(writeUrl) as HttpWebRequest;
httpRequest.Method = "PUT";
using (var dataStream = httpRequest.GetRequestStream())
{
    dataStream.Write(data.ToArray(), 0, (int)data.Length);
}
var response = httpRequest.GetResponse() as HttpWebResponse;
```

##### Deleting a container:
```
await _provider.DeleteContainerAsync(containerName);
```

##### Deleting a blob:
```
await _provider.DeleteBlobAsync(containerName, blobName);
```

## Adding Provider Accounts

The libary, and the functional tests within it, depends on access to actual cloud storage provider accounts.  Providing these configuration values can be done through environment variables or the user secret store.

### Installing User Secrets Manager
[Here](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.0) the instruction about the secret manager

Secrets can be listed with:
```
cd test\TwentyTwenty.Storage.Azure.Test
dotnet user-secrets list
```

### Adding Provider-Specific Secrets

##### Azure:
```
cd test\TwentyTwenty.Storage.Azure.Test
dotnet user-secrets set ConnectionString "my_really_long_connection_string"
```
##### Amazon:
```
cd test\TwentyTwenty.Storage.Amazon.Test
dotnet user-secrets set PublicKey "my_public_key"
dotnet user-secrets set PrivateKey "my_private_key"
dotnet user-secrets set Bucket "my_bucket_name"
dotnet user-secrets set ServerSideEncryptionMethod "AES256"
dotnet user-secrets set ProfileName "my_profile_name"
```
##### Google:
```
cd test\TwentyTwenty.Storage.Google.Test
dotnet user-secrets set GoogleEmail "my_google_storage_api_email"
dotnet user-secrets set GoogleBucket "my_google_bucket"
dotnet user-secrets set GoogleP12PrivateKey "my_base64_encoded_byte_array_google_p12_key"
```
(To get the 'GoogleP12PrivateKey' get the byte array from the P12 certificate for your Google Cloud Storage API and base64 encode it)
