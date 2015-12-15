using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TwentyTwenty.Storage.Google
{
    internal class GoogleSignedUrlGenerator
    {
        private readonly X509Certificate2 _cert;
        private readonly string _serviceEmail;
        private readonly string _bucket;

        public GoogleSignedUrlGenerator(X509Certificate2 cert, string serviceEmail, string bucket)
        {
            _bucket = bucket;
            _serviceEmail = serviceEmail;
            _cert = cert;
        }

        public string GetSignedUrl(string blob, DateTimeOffset expiry, string contentType = null, string fileName = null, string type = "GET")
        {
            var expiration = expiry.ToUnixTimeSeconds();
            var disp = fileName != null ? "content-disposition:attachment;filename=" + fileName : string.Empty;
            var urlSignature = SignString($"{type}\n\n{contentType}\n{expiration}\n/{_bucket}/{blob}");

            return $"https://storage.googleapis.com/{_bucket}/{blob}?GoogleAccessId={_serviceEmail}&Expires={expiration}&Signature={WebUtility.UrlEncode(urlSignature)}";
        }

        private string SignString(string stringToSign)
        {
            if (_cert == null)
            {
                throw new Exception("Certificate not initialized");
            }

            var cp = new CspParameters(24, "Microsoft Enhanced RSA and AES Cryptographic Provider",
                    ((RSACryptoServiceProvider)_cert.PrivateKey).CspKeyContainerInfo.KeyContainerName);
            var provider = new RSACryptoServiceProvider(cp);
            var buffer = Encoding.UTF8.GetBytes(stringToSign);
            var signature = provider.SignData(buffer, "SHA256");
            return Convert.ToBase64String(signature);
        }
    }
}