using System;

namespace Company.Product.Models.Responses
{
    public class HttpErrorResponse
    {
        public HttpErrorResponse()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public int Status { get; set; }

        public string Message { get; set; }

        public string TraceId { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
