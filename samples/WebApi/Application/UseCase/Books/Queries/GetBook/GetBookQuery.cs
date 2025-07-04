using Domain;
using MitMediator;

namespace Application.UseCase.Books.Queries.GetBook;

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