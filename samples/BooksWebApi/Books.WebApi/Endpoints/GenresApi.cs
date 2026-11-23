using Books.Application.UseCase.Genres.Commands.CreateGenre;
using Books.Application.UseCase.Genres.Commands.DeleteGenre;
using Books.Application.UseCase.Genres.Queries.GetGenres;
using Books.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MitMediator;

namespace Books.WebApi.Endpoints;

/// <summary>
/// Genres api endpoints.
/// </summary>
public static class GenresApi
{
    private const string Tag = "genres";
    
    private const string ApiUrl = "api";
    
    private const string Version = "v1";
    
    /// <summary>
    /// Use genres api endpoints.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseGenresApi(this WebApplication app)
    {
        var genres = app.MapGroup($"/{ApiUrl}/{Version}/{Tag}")
            .WithTags(Tag)
            .WithGroupName(Version);

        #region Queries
        
        genres.MapGet("/", GetAllGenresAsync)
            .WithName("GetGenres")
            .WithSummary("Gets all genres.");
        
        #endregion

        #region Commands

        genres.MapPost("/", CreateGenreAsync)
            .WithName("CreateGenre")
            .WithSummary("Creates a genre.");
        
        genres.MapDelete("/{genreName}", DeleteGenreAsync)
            .WithName("DeleteGenre")
            .WithSummary("Deletes a genre.");

        #endregion

        return app;
    }
    
    private static async ValueTask<Ok<Genre[]>> GetAllGenresAsync(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var genres = await mediator.SendAsync<GetGenresQuery, Genre[]>(new GetGenresQuery(), cancellationToken);
        return TypedResults.Ok(genres);
    }
    
    private static async ValueTask<Created<Genre>> CreateGenreAsync(
        [FromServices] IMediator mediator,
        [FromBody] CreateGenreCommand command,
        CancellationToken cancellationToken)
    {
        var genre = await mediator.SendAsync<CreateGenreCommand, Genre>(command, cancellationToken);
        return TypedResults.Created($"/api/v1/genres/{Uri.EscapeDataString(genre.GenreName)}", genre);
    }
    
    private static async ValueTask<NoContent> DeleteGenreAsync(
        [FromServices] IMediator mediator,
        [FromRoute] string genreName,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteGenreCommand() { GenreName = genreName}, cancellationToken);
        return TypedResults.NoContent();
    }
}