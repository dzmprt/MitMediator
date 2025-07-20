using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Books.Queries.GetBook;

/// <summary>
/// Get book query.
/// </summary>
public struct GetBookQuery : IRequest<Book>
{
    /// <summary>
    /// Book id.
    /// </summary>
    public int BookId { get; set; }
}