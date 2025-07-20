using Books.Application.UseCase.Authors.Commands.CreateAuthor;
using Books.Application.UseCase.Authors.Commands.DeleteAuthor;
using Books.Application.UseCase.Authors.Commands.UpdateAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Books.Domain;
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
        #region Queries
        
        app.MapGet($"{ApiUrl}/{Version}/{Tag}/{{authorId}}", GetAuthorByIdAsync)
            .WithTags(Tag)
            .WithName("Get author by id.")
            .WithGroupName(Version)
            .Produces<Author>();
        
        app.MapGet($"{ApiUrl}/{Version}/{Tag}", GetAuthorsByFilterAsync)
            .WithTags(Tag)
            .WithName("Get authors by filter.")
            .WithGroupName(Version)
            .Produces<Author[]>();
        
        #endregion

        #region Commands

        app.MapPost($"{ApiUrl}/{Version}/{Tag}", CreateAuthorAsync)
            .WithTags(Tag)
            .WithName("Create author.")
            .WithGroupName(Version)
            .Produces<Author>();
        
        app.MapPut($"{ApiUrl}/{Version}/{Tag}/{{authorId}}", UpdateAuthorAsync)
            .WithTags(Tag)
            .WithName("Update author.")
            .WithGroupName(Version)
            .Produces<Author>();
        
        app.MapDelete($"{ApiUrl}/{Version}/{Tag}/{{authorId}}", DeleteAuthorAsync)
            .WithTags(Tag)
            .WithName("Delete author.")
            .WithGroupName(Version);

        #endregion

        return app;
    }
    
    private static ValueTask<Author> GetAuthorByIdAsync([FromServices] IMediator mediator, [FromRoute] int authorId, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<GetAuthorQuery, Author>(new GetAuthorQuery()
        {
            AuthorId = authorId,
        }, cancellationToken);
    }
    
    private static ValueTask<Author[]> GetAuthorsByFilterAsync([FromServices] IMediator mediator, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? freeText, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<GetAuthorsByFilterQuery, Author[]>(new GetAuthorsByFilterQuery()
        {
            Limit = limit,
            Offset = offset,
            FreeText = freeText
        }, cancellationToken);
    }
    
    private static ValueTask<Author> CreateAuthorAsync([FromServices] IMediator mediator, [FromBody] CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<CreateAuthorCommand, Author>(command, cancellationToken);
    }
    
    private static ValueTask<Author> UpdateAuthorAsync([FromServices] IMediator mediator, [FromRoute] int authorId, [FromBody] UpdateAuthorPayload payload, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<UpdateAuthorCommand, Author>(payload.CreateCommand(authorId), cancellationToken);
    }
    
    private static async ValueTask DeleteAuthorAsync([FromServices] IMediator mediator, [FromRoute] int authorId, CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteAuthorCommand {AuthorId = authorId}, cancellationToken);
    }

    /// <summary>
    /// Update author payload.
    /// </summary>
    class UpdateAuthorPayload
    {
        /// <summary>
        /// New author first name.
        /// </summary>
        public string FirstName { get; init; } = null!;

        /// <summary>
        /// New author last name.
        /// </summary>
        public string LastName { get; init; } = null!;

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