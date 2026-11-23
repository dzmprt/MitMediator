using Books.Application.UseCase.Authors.Commands.CreateAuthor;
using Books.Application.UseCase.Authors.Commands.DeleteAuthor;
using Books.Application.UseCase.Authors.Commands.UpdateAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Books.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MitMediator;

namespace Books.WebApi.Endpoints;

/// <summary>
/// Authors api endpoints.
/// </summary>
internal static class AuthorsApi
{
    private const string Tag = "authors";
    
    private const string ApiUrl = "api";
    
    private const string Version = "v1";
    
    /// <summary>
    /// Use authors api endpoints.
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/>.</param>
    /// <returns><see cref="WebApplication"/>.</returns>
    public static WebApplication UseAuthorsApi(this WebApplication app)
    {
        var authors = app.MapGroup($"/{ApiUrl}/{Version}/{Tag}")
            .WithTags(Tag)
            .WithGroupName(Version);

        #region Queries
        
        authors.MapGet("/{authorId:int}", GetAuthorByIdAsync)
            .WithName("GetAuthorById")
            .WithSummary("Gets an author by id.");
        
        authors.MapGet("/", GetAuthorsByFilterAsync)
            .WithName("GetAuthors")
            .WithSummary("Gets authors matching an optional text filter.");
        
        #endregion

        #region Commands

        authors.MapPost("/", CreateAuthorAsync)
            .WithName("CreateAuthor")
            .WithSummary("Creates an author.");
        
        authors.MapPut("/{authorId:int}", UpdateAuthorAsync)
            .WithName("UpdateAuthor")
            .WithSummary("Updates an author.");
        
        authors.MapDelete("/{authorId:int}", DeleteAuthorAsync)
            .WithName("DeleteAuthor")
            .WithSummary("Deletes an author.");

        #endregion

        return app;
    }
    
    private static async ValueTask<Ok<Author>> GetAuthorByIdAsync(
        [FromServices] IMediator mediator,
        [FromRoute] int authorId,
        CancellationToken cancellationToken)
    {
        var author = await mediator.SendAsync<GetAuthorQuery, Author>(new GetAuthorQuery
        {
            AuthorId = authorId,
        }, cancellationToken);
        return TypedResults.Ok(author);
    }
    
    private static async ValueTask<Ok<Author[]>> GetAuthorsByFilterAsync(
        [FromServices] IMediator mediator,
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        [FromQuery] string? freeText,
        CancellationToken cancellationToken)
    {
        var authors = await mediator.SendAsync<GetAuthorsByFilterQuery, Author[]>(new GetAuthorsByFilterQuery
        {
            Limit = limit,
            Offset = offset,
            FreeText = freeText
        }, cancellationToken);
        return TypedResults.Ok(authors);
    }
    
    private static async ValueTask<Created<Author>> CreateAuthorAsync(
        [FromServices] IMediator mediator,
        [FromBody] CreateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = await mediator.SendAsync<CreateAuthorCommand, Author>(command, cancellationToken);
        return TypedResults.Created($"/api/v1/authors/{author.AuthorId}", author);
    }
    
    private static async ValueTask<Ok<Author>> UpdateAuthorAsync(
        [FromServices] IMediator mediator,
        [FromRoute] int authorId,
        [FromBody] UpdateAuthorPayload payload,
        CancellationToken cancellationToken)
    {
        var author = await mediator.SendAsync<UpdateAuthorCommand, Author>(
            payload.CreateCommand(authorId),
            cancellationToken);
        return TypedResults.Ok(author);
    }
    
    private static async ValueTask<NoContent> DeleteAuthorAsync(
        [FromServices] IMediator mediator,
        [FromRoute] int authorId,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteAuthorCommand {AuthorId = authorId}, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Update author payload.
    /// </summary>
    private sealed record UpdateAuthorPayload
    {
        /// <summary>
        /// New author first name.
        /// </summary>
        public required string FirstName { get; init; }

        /// <summary>
        /// New author last name.
        /// </summary>
        public required string LastName { get; init; }

        public UpdateAuthorCommand CreateCommand(int authorId)
        {
            return new UpdateAuthorCommand()
            {
                FirstName = FirstName,
                LastName = LastName,
                AuthorId = authorId
            };
        }
    }
}