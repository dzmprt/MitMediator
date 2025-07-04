using System.Net;
using Application.Exceptions;
using FluentValidation;

namespace WebApi.Middlewares;

/// <summary>
/// Custom exceptions handler middleware.
/// </summary>
internal class CustomExceptionsHandlerMiddleware
{
    private readonly RequestDelegate _next;
    
    /// <summary>
    /// Initializes a new instance of <see cref="CustomExceptionsHandlerMiddleware"/>.
    /// </summary>
    /// <param name="next">Next delefate.</param>
    public CustomExceptionsHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    
    /// <summary>
    /// Invoke handler.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/>.</param>
    /// <param name="logger"><see cref="ILogger"/></param>
    public async Task Invoke(HttpContext context, ILogger<CustomExceptionsHandlerMiddleware> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (!await HandleExceptionAsync(context, exception, logger))
            {
                throw;
            }
        }
    }

    private static async Task<bool> HandleExceptionAsync(HttpContext context, Exception exception, ILogger<CustomExceptionsHandlerMiddleware> logger)
    {
        HttpStatusCode code;
        string result;
        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = System.Text.Json.JsonSerializer.Serialize(validationException.Errors);
                break;
            case BadOperationException badOperationException:
                code = HttpStatusCode.BadRequest;
                result = System.Text.Json.JsonSerializer.Serialize(badOperationException.Message);
                break;
            case NotFoundException notFound:
                code = HttpStatusCode.NotFound;
                result = System.Text.Json.JsonSerializer.Serialize(notFound.Message);
                break;
            default:
                return false;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        if (result == string.Empty)
            result = System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message, innerMessage = exception.InnerException?.Message, exception.StackTrace });
        logger.Log(LogLevel.Warning, exception, $"Response error {code}: {exception.Message}");

        await context.Response.WriteAsync(result);
        return true;
    }
}

/// <summary>
/// Custom exceptions handler middleware extensions.
/// </summary>
internal static class CustomExceptionsHandlerMiddlewareExtensions
{
    /// <summary>
    /// Use custom exceptions handler.
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/>.</param>
    /// <returns><see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseCustomExceptionsHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionsHandlerMiddleware>();
    }
}