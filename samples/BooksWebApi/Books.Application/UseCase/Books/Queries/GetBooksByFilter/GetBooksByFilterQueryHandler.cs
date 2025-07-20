using Books.Application.Abstractions.Infrastructure;
using Books.Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Books.Queries.GetBooksByFilter;

/// <summary>
/// Handler for <see cref="GetBooksByFilterQuery"/>.
/// </summary>
internal sealed class GetBooksByFilterQueryHandler : IRequestHandler<GetBooksByFilterQuery, Book[]>
{
    private readonly IBaseProvider<Book> _booksProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorsByFilterQueryHandler"/>.
    /// </summary>
    /// <param name="booksProvider">Books provider.</param>
    public GetBooksByFilterQueryHandler(IBaseProvider<Book> booksProvider)
    {
        _booksProvider = booksProvider;
    }
    
    /// <inheritdoc/>
    public ValueTask<Book[]> HandleAsync(GetBooksByFilterQuery request, CancellationToken cancellationToken)
    {
        var freeText = request.FreeText?.Trim().ToUpperInvariant();
        return _booksProvider.SearchAsync(
            freeText is null
                ? null
                : q => q.Author.FirstName.Contains(freeText) || 
                       q.Author.LastName.Contains(freeText) || 
                       q.Title.Contains(freeText) || 
                       q.Genre.GenreName.Contains(freeText),
            o => o.BookId,
            request.Limit,
            request.Offset,
            cancellationToken
        );
    }
}