using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Books.Commands.UpdateBook;

/// <summary>
/// Update book command.
/// </summary>
public class UpdateBookCommand : IRequest<Book>
{
    /// <summary>
    /// Book id.
    /// </summary>
    public int BookId { get; init; }
    
    /// <summary>
    /// Title.
    /// </summary>
    public string Title { get; init; }
    
    /// <summary>
    /// Author id.
    /// </summary>
    public int AuthorId { get; init; }
    
    /// <summary>
    /// Genre.
    /// </summary>
    public string GenreName { get; init; }

}