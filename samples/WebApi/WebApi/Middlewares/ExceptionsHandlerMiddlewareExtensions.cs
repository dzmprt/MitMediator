using System.Net;
using Application.Exceptions;
using FluentValidation;

namespace WebApi.Middlewares;

internal class CoreExceptionsHandlerMiddleware
{
    private readonly RequestDelegate _next;
    
    public CoreExceptionsHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context, ILogger<CoreExceptionsHandlerMiddleware> logger)
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

    private static async Task<bool> HandleExceptionAsync(HttpContext context, Exception exception, ILogger<CoreExceptionsHandlerMiddleware> logger)
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

internal static class ExceptionsHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionsHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CoreExceptionsHandlerMiddleware>();
    }
}