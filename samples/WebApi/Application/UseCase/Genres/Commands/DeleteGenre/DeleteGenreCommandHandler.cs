using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Domain;
using MitMediator;

namespace Application.UseCase.Genres.Commands.DeleteGenre;

/// <summary>
/// Handler for <see cref="DeleteGenreCommand"/>.
/// </summary>
internal sealed class DeleteGenreCommandHandler : IRequestHandler<DeleteGenreCommand>
{
    private readonly IBaseRepository<Genre> _genreRepository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteGenreCommandHandler"/>.
    /// </summary>
    /// <param name="genreRepository">Genre repository.</param>
    public DeleteGenreCommandHandler(IBaseRepository<Genre> genreRepository)
    {
        _genreRepository = genreRepository;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Unit> HandleAsync(DeleteGenreCommand command, CancellationToken cancellationToken)
    {
        var genreName = command.GenreName.ToUpperInvariant();
        var genre = await _genreRepository.FirstOrDefaultAsync(q => q.GenreName == genreName, cancellationToken);
        if (genre is null)
        {
            throw new NotFoundException();
        }
        await _genreRepository.RemoveAsync(genre, cancellationToken);
        return Unit.Value;
    }
}