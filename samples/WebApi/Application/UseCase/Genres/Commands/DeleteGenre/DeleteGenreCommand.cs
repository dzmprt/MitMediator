using MitMediator;

namespace Application.UseCase.Genres.Commands.DeleteGenre;

/// <summary>
/// Delete genre command.
/// </summary>
public struct DeleteGenreCommand : IRequest
{
    /// <summary>
    /// Genre name.
    /// </summary>
    public string GenreName { get; init; }
}