using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Domain;
using MitMediator;

namespace Application.UseCase.Books.Queries.GetBook;

/// <summary>
/// Handler for <see cref="GetBookQuery"/>.
/// </summary>
internal sealed class GetBookQueryHandler : IRequestHandler<GetBookQuery, Book>
{
    private readonly IBaseProvider<Book> _bookProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookQueryHandler"/>.
    /// </summary>
    /// <param name="bookProvider">Books provider.</param>
    public GetBookQueryHandler(IBaseProvider<Book> bookProvider)
    {
        _bookProvider = bookProvider;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Book> HandleAsync(GetBookQuery query, CancellationToken cancellationToken)
    {
        var book = await _bookProvider.FirstOrDefaultAsync(q => q.BookId == query.BookId, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException();
        }

        return book;
    }
}