using System.Net;
using System.Text.Json;
using PMS.Core.Helpers;
namespace PMS.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var traceId = context.TraceIdentifier;

            // Map exception type -> status code + client-safe message
            var (statusCode, message) = ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                ArgumentNullException => (HttpStatusCode.NotFound, "The requested resource was not found."),  // ✅ added
                InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized to perform this action."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
            };

            // Log full details ALWAYS (internal only — console/file, never sent to client)
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}, Path: {Path}",
                    traceId, context.Request.Path);
            }
            else
            {
                _logger.LogWarning("Handled exception: {ExceptionType}. TraceId: {TraceId}, Path: {Path}, Message: {Message}",
                    ex.GetType().Name, traceId, context.Request.Path, ex.Message);
            }

            // Build the response using object initializer style — same pattern as controllers
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = _env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError
                    ? new List<string> { ex.ToString() }
                    : null,
                StatusCode = (int)statusCode,
                TraceId = traceId
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

}
