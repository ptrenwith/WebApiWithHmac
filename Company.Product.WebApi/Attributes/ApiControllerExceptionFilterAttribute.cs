using Company.Product.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;

namespace Company.Product.WebApi.Attributes
{
    public class ApiControllerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiControllerExceptionFilterAttribute> _logger;

        public ApiControllerExceptionFilterAttribute(ILogger<ApiControllerExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);

            var code = context.Exception switch
            {
                ArgumentNullException ex => HttpStatusCode.BadRequest,
                ArgumentException ex => HttpStatusCode.BadRequest,
                UnauthorizedAccessException ex => HttpStatusCode.Unauthorized,
                NotImplementedException ex => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError,
            };
            var statusCode = (int)code;
            var message = context.Exception.Message;

            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                var validationMessage = string.Join(", ", context.ModelState.Values.SelectMany(x => x.Errors).Select(x => $"[{ x.ErrorMessage }]"));
                message = string.IsNullOrEmpty(validationMessage) ? message : validationMessage;
            }

            var result = new HttpErrorResponse
            {
                Status = statusCode,
                Message = message,
                TraceId = context.HttpContext.TraceIdentifier
            };

            _logger.LogError($"{nameof(ApiControllerExceptionFilterAttribute)} response: {result}");
            context.HttpContext.Response.StatusCode = statusCode;
            context.Result = new ObjectResult(result);
        }
    }
}
