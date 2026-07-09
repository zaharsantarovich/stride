using System.Net;
using System.Text.Json;
using ZSLabs.Stride.Api.Contracts;

namespace ZSLabs.Stride.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async global::System.Threading.Tasks.Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled request failure.");

            var statusCode = exception switch
            {
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Forbidden,
                InvalidOperationException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError,
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse(exception.Message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
        }
    }
}