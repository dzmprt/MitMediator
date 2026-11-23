using Books.Application.Exceptions;
using Books.WebApi.Middlewares;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Books.WebApi.Tests;

public sealed class ExceptionHandlerTests
{
    [Theory]
    [MemberData(nameof(ExceptionCases))]
    public async Task Handler_MapsKnownAndUnknownExceptions(Exception exception, int expectedStatus)
    {
        var captured = default(ProblemDetails);
        var service = new Mock<IProblemDetailsService>();
        service
            .Setup(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(context => captured = context.ProblemDetails)
            .Returns(new ValueTask<bool>(true));
        var handler = new BooksApiExceptionHandler(
            NullLogger<BooksApiExceptionHandler>.Instance,
            service.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatus, httpContext.Response.StatusCode);
        Assert.Equal(expectedStatus, captured!.Status);
        Assert.Equal("/test", captured.Instance);
        if (exception is ValidationException)
        {
            Assert.IsType<HttpValidationProblemDetails>(captured);
        }
    }

    public static TheoryData<Exception, int> ExceptionCases => new()
    {
        {
            new ValidationException(
                [new ValidationFailure("Title", "Title is required.")]),
            StatusCodes.Status400BadRequest
        },
        { new BadOperationException("Invalid operation"), StatusCodes.Status400BadRequest },
        { new NotFoundException(), StatusCodes.Status404NotFound },
        { new InvalidOperationException("Unexpected"), StatusCodes.Status500InternalServerError }
    };
}
