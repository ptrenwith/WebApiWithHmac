using Company.Product.AuthenticationService;
using Company.Product.Models.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Newtonsoft.Json;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace Company.Product.WebApi.Authentication
{
    internal sealed class HmacAuthenticationHandler : AuthenticationHandler<HmacAuthenticationOptions>
    {
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public HmacAuthenticationHandler(
            IOptionsMonitor<HmacAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            bool isValid = false;
            var context = Request.HttpContext;
            var authorization = Request.Headers["Authorization"];

            var authorizationHeaderArray = GetAuthorizationHeaderValues(authorization);

            if (authorizationHeaderArray != null)
            {
                var appId = authorizationHeaderArray[0];
                var incomingBase64Signature = authorizationHeaderArray[1];
                var nonce = authorizationHeaderArray[2];
                var requestTimeStamp = authorizationHeaderArray[3];
                var absoluteRoute = (context.Request.Scheme + "://" + context.Request.Host + context.Request.Path).ToLower();
                if (!string.IsNullOrEmpty(context.Request.QueryString.Value))
                {
                    absoluteRoute += context.Request.QueryString.Value;
                }
                string requestUri = HttpUtility.UrlEncode(absoluteRoute);
                using var requestStream = _recyclableMemoryStreamManager.GetStream();
                context.Request.Body.Position = 0;
                await context.Request.Body.CopyToAsync(requestStream);
                var body = ReadStreamInChunks(requestStream);
                byte[] content = Encoding.ASCII.GetBytes(body);
                var hashedContent = HMACAuthentication.Sha256(content);

                isValid = HMACAuthentication.ValidateHMACSignature(context.Request.Method,
                    requestUri, appId, requestTimeStamp, nonce, hashedContent, incomingBase64Signature);
            }

            if (isValid)
            {
                context.Request.Body.Position = 0; // rewind the buffer
                var currentPrincipal = new GenericPrincipal(new GenericIdentity(HMACAuthentication.AppId), null);
                var ticket = new AuthenticationTicket(currentPrincipal, new AuthenticationProperties { IsPersistent = false }, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            var result = JsonConvert.SerializeObject(new HttpErrorResponse
            {
                Status = (int)StatusCodes.Status401Unauthorized,
                Message = "Unauthorized",
                TraceId = context.TraceIdentifier
            }, Formatting.Indented);

            return AuthenticateResult.Fail(result);
        }

        private string[] GetAuthorizationHeaderValues(string headers)
        {
            var credArray = headers?.Split(':');
            if (credArray?.Length == 4)
            {
                credArray[0] = credArray[0].Split(" ")[1].Trim();
                return credArray;
            }
            else
            {
                return null;
            }
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            var result = string.Empty;
            using (var textWriter = new StringWriter())
            {
                using (var reader = new StreamReader(stream))
                {
                    var readChunk = new char[readChunkBufferLength];
                    int readChunkLength;
                    do
                    {
                        readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                        textWriter.Write(readChunk, 0, readChunkLength);
                    }
                    while (readChunkLength > 0);
                }
                result = textWriter.ToString();
            }
            return result;
        }
    }
}
