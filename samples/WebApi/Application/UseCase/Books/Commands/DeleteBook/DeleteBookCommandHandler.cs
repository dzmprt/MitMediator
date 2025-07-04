using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Application.UseCase.Authors.Commands.DeleteAuthor;
using Domain;
using MitMediator;

namespace Application.UseCase.Books.Commands.DeleteBook;

/// <summary>
/// Handler for <see cref="DeleteBookCommand"/>.
/// </summary>
internal sealed class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand>
{
    private readonly IBaseRepository<Book> _booksRepository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookCommandHandler"/>.
    /// </summary>
    /// <param name="booksRepository">Books repository.</param>
    public DeleteBookCommandHandler(IBaseRepository<Book> booksRepository)
    {
        _booksRepository = booksRepository;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Unit> HandleAsync(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _booksRepository.FirstOrDefaultAsync(q => q.BookId == command.BookId, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException();
        }
        await _booksRepository.RemoveAsync(book, cancellationToken);
        return Unit.Value;
    }
}