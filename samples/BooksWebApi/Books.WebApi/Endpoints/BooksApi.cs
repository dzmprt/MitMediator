using Books.Application.UseCase.Books.Commands.CreateBook;
using Books.Application.UseCase.Books.Commands.DeleteBook;
using Books.Application.UseCase.Books.Commands.UpdateBook;
using Books.Application.UseCase.Books.Queries.GetBook;
using Books.Application.UseCase.Books.Queries.GetBooksByFilter;
using Books.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MitMediator;

namespace Books.WebApi.Endpoints;

/// <summary>
/// Books api endpoints.
/// </summary>
public static class BooksApi
{
    private const string Tag = "books";
    
    private const string ApiUrl = "api";
    
    private const string Version = "v1";
    
    /// <summary>
    /// Use books api endpoints.
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/>.</param>
    /// <returns><see cref="WebApplication"/>.</returns>
    public static WebApplication UseBooksApi(this WebApplication app)
    {
        var books = app.MapGroup($"/{ApiUrl}/{Version}/{Tag}")
            .WithTags(Tag)
            .WithGroupName(Version);

        #region Queries
        
        books.MapGet("/{bookId:int}", GetBookByIdAsync)
            .WithName("GetBookById")
            .WithSummary("Gets a book by id.");
        
        books.MapGet("/", GetBooksByFilterAsync)
            .WithName("GetBooks")
            .WithSummary("Gets books matching an optional text filter.");
        
        #endregion

        #region Commands

        books.MapPost("/", CreateBookAsync)
            .WithName("CreateBook")
            .WithSummary("Creates a book.");
        
        books.MapPut("/{bookId:int}", UpdateBookAsync)
            .WithName("UpdateBook")
            .WithSummary("Updates a book.");
        
        books.MapDelete("/{bookId:int}", DeleteBookAsync)
            .WithName("DeleteBook")
            .WithSummary("Deletes a book.");

        #endregion

        return app;
    }
    
    private static async ValueTask<Ok<Book>> GetBookByIdAsync(
        [FromServices] IMediator mediator,
        [FromRoute] int bookId,
        CancellationToken cancellationToken)
    {
        var book = await mediator.SendAsync<GetBookQuery, Book>(new GetBookQuery
        {
            BookId = bookId,
        }, cancellationToken);
        return TypedResults.Ok(book);
    }
    
    private static async ValueTask<Ok<Book[]>> GetBooksByFilterAsync(
        [FromServices] IMediator mediator,
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        [FromQuery] string? freeText,
        CancellationToken cancellationToken)
    {
        var books = await mediator.SendAsync<GetBooksByFilterQuery, Book[]>(new GetBooksByFilterQuery
        {
            Limit = limit,
            Offset = offset,
            FreeText = freeText
        }, cancellationToken);
        return TypedResults.Ok(books);
    }
    
    private static async ValueTask<Created<Book>> CreateBookAsync(
        [FromServices] IMediator mediator,
        [FromBody] CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await mediator.SendAsync<CreateBookCommand, Book>(command, cancellationToken);
        return TypedResults.Created($"/api/v1/books/{book.BookId}", book);
    }
    
    private static async ValueTask<Ok<Book>> UpdateBookAsync(
        [FromServices] IMediator mediator,
        [FromRoute] int bookId,
        [FromBody] UpdateBookPayload payload,
        CancellationToken cancellationToken)
    {
        var book = await mediator.SendAsync<UpdateBookCommand, Book>(
            payload.CreateCommand(bookId),
            cancellationToken);
        return TypedResults.Ok(book);
    }
    
    private static async ValueTask<NoContent> DeleteBookAsync(
        [FromServices] IMediator mediator,
        [FromRoute] int bookId,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteBookCommand() {BookId = bookId}, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Update author payload.
    /// </summary>
    private sealed record UpdateBookPayload
    {
        /// <summary>
        /// Title.
        /// </summary>
        public required string Title { get; init; }
    
        /// <summary>
        /// Author id.
        /// </summary>
        public int AuthorId { get; init; }
    
        /// <summary>
        /// Genre.
        /// </summary>
        public required string GenreName { get; init; }

        public UpdateBookCommand CreateCommand(int bookId)
        {
            return new UpdateBookCommand()
            {
                GenreName = GenreName,
                Title = Title,
                AuthorId = AuthorId,
                BookId = bookId
            };
        }
    }
}