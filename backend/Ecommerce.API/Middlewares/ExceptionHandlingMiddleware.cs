using Ecommerce.Shared.Exceptions;
using System;
using System.Net;
using System.Text.Json;

namespace Ecommerce.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode;
            var result = new
            {
                message = ex.Message
            };

            switch (ex)
            {
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                case InvalidLoginException:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                case InvalidOwnershipException:
                    statusCode = HttpStatusCode.Forbidden;
                    break;
                case ArgumentException:
                case ValidationException:
                case InvalidProductException:
                case InvalidVendorException:
                case InvalidAncestorException:
                case InvalidOperationException:
                case InsufficientStockException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case UniquenessViolationException:
                    statusCode = HttpStatusCode.Conflict;
                    break;
                case InvalidEmailCredsException:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
