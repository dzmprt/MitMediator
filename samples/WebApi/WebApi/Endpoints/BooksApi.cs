using Application.UseCase.Authors.Commands.CreateAuthor;
using Application.UseCase.Authors.Commands.DeleteAuthor;
using Application.UseCase.Authors.Commands.UpdateAuthor;
using Application.UseCase.Authors.Queries.GetAuthor;
using Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Application.UseCase.Books.Commands.CreateBook;
using Application.UseCase.Books.Commands.DeleteBook;
using Application.UseCase.Books.Commands.UpdateBook;
using Application.UseCase.Books.Queries.GetBook;
using Application.UseCase.Books.Queries.GetBooksByFilter;
using Domain;
using Microsoft.AspNetCore.Mvc;
using MitMediator;

namespace WebApi.Endpoints;

public static class BooksApi
{
    private const string Tag = "books";
    
    private const string ApiUrl = "api";
    
    private const string Version = "v1";
    
    public static WebApplication UseBooksApi(this WebApplication app)
    {
        #region Queries
        
        app.MapGet($"{ApiUrl}/{Version}/{Tag}/{{bookId}}", GetBookByIdAsync)
            .WithTags(Tag)
            .WithName("Get book by id.")
            .WithGroupName(Version)
            .Produces<Book>();
        
        app.MapGet($"{ApiUrl}/{Version}/{Tag}", GetBooksByFilterAsync)
            .WithTags(Tag)
            .WithName("Get books by filter.")
            .WithGroupName(Version)
            .Produces<Book[]>();
        
        #endregion

        #region Commands

        app.MapPost($"{ApiUrl}/{Version}/{Tag}", CreateBookAsync)
            .WithTags(Tag)
            .WithName("Create book.")
            .WithGroupName(Version)
            .Produces<Book>();
        
        app.MapPut($"{ApiUrl}/{Version}/{Tag}/{{bookId}}", UpdateBookAsync)
            .WithTags(Tag)
            .WithName("Update book.")
            .WithGroupName(Version)
            .Produces<Book>();
        
        app.MapDelete($"{ApiUrl}/{Version}/{Tag}/{{bookId}}", DeleteBookAsync)
            .WithTags(Tag)
            .WithName("Delete book.")
            .WithGroupName(Version);

        #endregion

        return app;
    }
    
    private static ValueTask<Book> GetBookByIdAsync([FromServices] IMediator mediator, [FromRoute] int bookId, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<GetBookQuery, Book>(new GetBookQuery()
        {
            BookId = bookId,
        }, cancellationToken);
    }
    
    private static ValueTask<Book[]> GetBooksByFilterAsync([FromServices] IMediator mediator, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? freeText, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<GetBooksByFilterQuery, Book[]>(new GetBooksByFilterQuery()
        {
            Limit = limit,
            Offset = offset,
            FreeText = freeText
        }, cancellationToken);
    }
    
    private static ValueTask<Book> CreateBookAsync([FromServices] IMediator mediator, [FromBody] CreateBookCommand command, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<CreateBookCommand, Book>(command, cancellationToken);
    }
    
    private static ValueTask<Book> UpdateBookAsync([FromServices] IMediator mediator, [FromRoute] int bookId, [FromBody] UpdateBookPayload payload, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<UpdateBookCommand, Book>(payload.CreateCommand(bookId), cancellationToken);
    }
    
    private static async ValueTask DeleteBookAsync([FromServices] IMediator mediator, [FromRoute] int bookId, CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteBookCommand() {BookId = bookId}, cancellationToken);
    }

    /// <summary>
    /// Update author payload.
    /// </summary>
    class UpdateBookPayload
    {
        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; init; }
    
        /// <summary>
        /// Author id.
        /// </summary>
        public int AuthorId { get; init; }
    
        /// <summary>
        /// Genre.
        /// </summary>
        public string GenreName { get; init; }

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