using System;
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

        public GoogleSignedUrlGenerator(string keyPath, string serviceEmail, string bucket)
        {
            _bucket = bucket;
            _serviceEmail = serviceEmail;
            _cert = new X509Certificate2(keyPath, "notasecret");
        }

        //private void Run()
        //{
        //    try
        //    {
        //        Console.WriteLine("======= PUT File =========");
        //        string put_url = this.GetSignedUrl("PUT");
        //        string payload = "Lorem ipsum";

        //        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(put_url);
        //        request.Method = "PUT";
        //        byte[] byte1 = new UTF8Encoding().GetBytes(payload);
        //        using (Stream reqStream = request.GetRequestStream())
        //        {
        //            reqStream.Write(byte1, 0, byte1.Length);
        //            Console.WriteLine(request.Method + " " + request.Host + request.RequestUri.PathAndQuery);
        //            renderResponse((HttpWebResponse)request.GetResponse());
        //        }

        //        Console.WriteLine("======= GET File =========");
        //        string get_url = this.GetSignedUrl("GET");
        //        request = (HttpWebRequest)HttpWebRequest.Create(get_url);
        //        request.Method = "GET";
        //        Console.WriteLine(request.Method + " " + request.Host + request.RequestUri.PathAndQuery);
        //        Console.WriteLine(renderResponse((HttpWebResponse)request.GetResponse()));

        //        Console.WriteLine("======= DELETE File =========");
        //        string delete_url = this.GetSignedUrl("DELETE");
        //        request = (HttpWebRequest)HttpWebRequest.Create(delete_url);
        //        request.Method = "DELETE";
        //        Console.WriteLine(request.Method + " " + request.Host + request.RequestUri.PathAndQuery);
        //        Console.WriteLine(renderResponse((HttpWebResponse)request.GetResponse()));
        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Status == WebExceptionStatus.ProtocolError)
        //        {
        //            HttpStatusCode statusCode = ((HttpWebResponse)ex.Response).StatusCode;
        //            string statusDescription = ((HttpWebResponse)ex.Response).StatusDescription;
        //            Console.WriteLine("HTTP Error: " + statusCode + " " + statusDescription);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception " + ex);
        //    }

        //}

        public string GetSignedUrl(string blob, DateTimeOffset expiry, string contentType = null, string fileName = null, string type = "GET")
        {
            var expiration = expiry.ToUnixTimeSeconds();
            var disp = fileName != null ? "attachment;filename=" + fileName : string.Empty;
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