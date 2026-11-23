using MitMediator;

namespace Books.Application.UseCase.Genres.Commands.DeleteGenre;

/// <summary>
/// Delete genre command.
/// </summary>
public struct DeleteGenreCommand : IRequest
{
    /// <summary>
    /// Genre name.
    /// </summary>
    public required string GenreName { get; init; }
}