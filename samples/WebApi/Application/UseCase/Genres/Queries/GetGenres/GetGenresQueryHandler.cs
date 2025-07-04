using Application.Abstractions.Infrastructure;
using Domain;
using MitMediator;

namespace Application.UseCase.Genres.Queries.GetGenres;

/// <summary>
/// Handler for <see cref="GetGenresQuery"/>.
/// </summary>
internal sealed class GetGenresQueryHandler : IRequestHandler<GetGenresQuery, Genre[]>
{
    private readonly IBaseProvider<Genre> _genreProvider;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenresQueryHandler"/>.
    /// </summary>
    /// <param name="genreProvider">Genre provider.</param>
    public GetGenresQueryHandler(IBaseProvider<Genre> genreProvider)
    {
        _genreProvider = genreProvider;
    }
    
    /// <inheritdoc/>
    public ValueTask<Genre[]> HandleAsync(GetGenresQuery request, CancellationToken cancellationToken)
    {
        return _genreProvider.GetAllAsync(cancellationToken);
    }
}