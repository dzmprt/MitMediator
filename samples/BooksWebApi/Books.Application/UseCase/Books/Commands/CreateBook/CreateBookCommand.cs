using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Books.Commands.CreateBook;

/// <summary>
/// Create book command.
/// </summary>
public struct CreateBookCommand : IRequest<Book>
{
    /// <summary>
    /// Title.
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Author id.
    /// </summary>
    public int AuthorId { get; init; }
    
    /// <summary>
    /// Genre.
    /// </summary>
    public required string GenreName { get; init; }

}