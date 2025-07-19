using MitMediator;

namespace Books.Application.UseCase.Books.Commands.DeleteBook;

/// <summary>
/// Delete book command.
/// </summary>
public struct DeleteBookCommand : IRequest
{
    /// <summary>
    /// Book id.
    /// </summary>
    public int BookId { get; init; }
}