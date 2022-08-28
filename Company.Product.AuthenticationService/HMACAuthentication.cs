using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Company.Product.AuthenticationService
{
    public sealed class HMACAuthentication
    {
        public static readonly string AppId = "65d3a4f0-0239-404c-8394-21b94ff50604";
        public static readonly string ApiKey = "WLUEWeL3so2hdHhHM5ZYnvzsOUBzSGH4+T3EgrQ91KI=";
        public static readonly string AuthenticationScheme = "hmacauth";
        private static Dictionary<string, string> _allowedApps = new Dictionary<string, string>
        {
            { AppId, ApiKey },
        };
        private readonly static int _requestMaxAgeInSeconds = 300;
        private static readonly MemoryCacheOptions _cacheOptions = new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromSeconds(_requestMaxAgeInSeconds),
        };
        private static readonly IMemoryCache _memoryCache = new MemoryCache(_cacheOptions);

        public static string ComputeHMACSignature(string httpMethod, string requestUri, string timestamp, string nonce, string content)
        {
            return ComputeHMACSignature(httpMethod, requestUri, AppId, ApiKey, timestamp, nonce, content);
        }

        public static string ComputeHMACSignature(string httpMethod, string requestUri, string appId, string key, string timestamp, string nonce, string content)
        {
            string signatureRawData = String.Format("{0}{1}{2}{3}{4}{5}", appId, httpMethod, requestUri, timestamp, nonce, content);
            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);
            var secretKeyByteArray = Convert.FromBase64String(key);
            string requestSignatureBase64String = String.Empty;

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
            }

            return requestSignatureBase64String;
        }

        public static bool ValidateHMACSignature(string httpMethod, string requestUri, string appId, string timestamp, string nonce, string content, string incomingBase64Signature)
        {
            if (!_allowedApps.ContainsKey(appId) || IsReplayRequest(nonce, timestamp))
            {
                return false;
            }

            var sharedKey = _allowedApps[appId];
            var computedSignature = ComputeHMACSignature(httpMethod, requestUri, appId, sharedKey, timestamp, nonce, content);
            return (incomingBase64Signature.Equals(computedSignature, StringComparison.Ordinal));
        }

        /// <summary>
        /// If the given nonce is present in the cache then this is a replay, replay is not permitted.
        /// </summary>
        /// <param name="nonce"></param>
        /// <param name="requestTimeStamp"></param>
        /// <returns></returns>
        private static bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            if (_memoryCache.TryGetValue(nonce, out string _))
            {
                return true;
            }
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs = DateTime.UtcNow - epochStart;
            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            if ((int)(serverTotalSeconds - requestTotalSeconds) > _requestMaxAgeInSeconds)
            {
                return true;
            }
            _memoryCache.Set(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(_requestMaxAgeInSeconds));
            return false;
        }

        /// <summary>
        /// Return a base64 representation of the SHA-256 hash of the content
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Sha256(byte[] content)
        {
            var result = string.Empty;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(content);
                result = Convert.ToBase64String(bytes);
            }
            return result;
        }
    }
}
