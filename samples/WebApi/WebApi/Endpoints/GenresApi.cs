using Application.UseCase.Genres.Commands.CreateGenre;
using Application.UseCase.Genres.Commands.DeleteGenre;
using Application.UseCase.Genres.Queries.GetGenres;
using Domain;
using Microsoft.AspNetCore.Mvc;
using MitMediator;

namespace WebApi.Endpoints;

public static class GenresApi
{
    private const string Tag = "genres";
    
    private const string ApiUrl = "api";
    
    private const string Version = "v1";
    
    public static WebApplication UseGenresApi(this WebApplication app)
    {
        #region Queries
        
        app.MapGet($"{ApiUrl}/{Version}/{Tag}", GetAllGenresAsync)
            .WithTags(Tag)
            .WithName("Get all genres.")
            .WithGroupName(Version)
            .Produces<Genre[]>();
        
        #endregion

        #region Commands

        app.MapPost($"{ApiUrl}/{Version}/{Tag}", CreateGenreAsync)
            .WithTags(Tag)
            .WithName("Create genre.")
            .WithGroupName(Version)
            .Produces<Author>();
        
        app.MapDelete($"{ApiUrl}/{Version}/{Tag}/{{genreName}}", DeleteGenreAsync)
            .WithTags(Tag)
            .WithName("Delete genre.")
            .WithGroupName(Version);

        #endregion

        return app;
    }
    
    private static ValueTask<Genre[]> GetAllGenresAsync([FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<GetGenresQuery, Genre[]>(new GetGenresQuery(), cancellationToken);
    }
    
    private static ValueTask<Genre> CreateGenreAsync([FromServices] IMediator mediator, [FromBody] CreateGenreCommand command, CancellationToken cancellationToken)
    {
        return mediator.SendAsync<CreateGenreCommand, Genre>(command, cancellationToken);
    }
    
    private static async ValueTask DeleteGenreAsync([FromServices] IMediator mediator, [FromRoute] string genreName, CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteGenreCommand() { GenreName = genreName}, cancellationToken);
    }
}