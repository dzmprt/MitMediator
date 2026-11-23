using Books.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Books.WebApi.Middlewares;

/// <summary>
/// Maps application exceptions to RFC 9457 problem details.
/// </summary>
internal sealed class BooksApiExceptionHandler(
    ILogger<BooksApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblem(validationException),
            BadOperationException => CreateProblem(StatusCodes.Status400BadRequest, "Bad operation"),
            NotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Not found"),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "Internal server error")
        };

        problemDetails.Instance = httpContext.Request.Path;
        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        logger.Log(
            problemDetails.Status >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : LogLevel.Warning,
            exception,
            "Request failed with status {StatusCode}",
            problemDetails.Status);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    private static HttpValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        return new HttpValidationProblemDetails(
            exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        };
    }

    private static ProblemDetails CreateProblem(int statusCode, string title)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };
    }
}
