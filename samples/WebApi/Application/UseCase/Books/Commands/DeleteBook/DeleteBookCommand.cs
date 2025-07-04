using MitMediator;

namespace Application.UseCase.Books.Commands.DeleteBook;

/// <summary>
/// Delete book command.
/// </summary>
public struct DeleteBookCommand : IRequest
{
    /// <summary>
    /// Author id.
    /// </summary>
    public int BookId { get; init; }
}