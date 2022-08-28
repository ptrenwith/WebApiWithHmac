using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Company.Product.AuthenticationService
{
    public sealed class HMACDelegatingHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            string requestContentBase64String = string.Empty;

            string requestUri = HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());
            string requestHttpMethod = request.Method.Method;

            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
            string nonce = Guid.NewGuid().ToString("N");

            if (request.Content != null)
            {
                byte[] content = await request.Content.ReadAsByteArrayAsync();
                requestContentBase64String = HMACAuthentication.Sha256(content);
            }

            var requestSignatureBase64String = HMACAuthentication.ComputeHMACSignature(requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);
            request.Headers.Authorization = new AuthenticationHeaderValue("hmacauth", string.Format("{0}:{1}:{2}:{3}", HMACAuthentication.AppId, requestSignatureBase64String, nonce, requestTimeStamp));
            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
