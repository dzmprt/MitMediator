using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Application.UseCase.Books.Commands.CreateBook;
using Domain;
using MitMediator;

namespace Application.UseCase.Books.Commands.UpdateBook;

/// <summary>
/// Handler for <see cref="CreateBookCommand"/>
/// </summary>
public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, Book>
{
    private readonly IBaseRepository<Book> _booksRepository;
    
    private readonly IBaseRepository<Author> _authorsRepository;
    
    private readonly IBaseRepository<Genre> _genresRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBookCommandHandler"/>.
    /// </summary>
    /// <param name="booksRepository">Books repository.</param>
    /// <param name="authorsRepository">Authors repository.</param>
    /// <param name="genresRepository">Genres repository.</param>
    public UpdateBookCommandHandler(
        IBaseRepository<Book> booksRepository, 
        IBaseRepository<Author> authorsRepository,
        IBaseRepository<Genre> genresRepository)
    {
        _booksRepository = booksRepository;
        _authorsRepository = authorsRepository;
        _genresRepository = genresRepository;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Book> HandleAsync(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _booksRepository.FirstOrDefaultAsync(b => b.BookId == request.BookId, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException();
        }
        
        var author = await _authorsRepository.FirstOrDefaultAsync(a => a.AuthorId == request.AuthorId, cancellationToken);
        if (author is null)
        {
            throw new BadOperationException("Author not found");
        }
        var genre = await _genresRepository.FirstOrDefaultAsync(g => g.GenreName == request.GenreName.Trim().ToUpperInvariant(), cancellationToken);
        if (genre is null)
        {
            throw new BadOperationException("Genre not found");
        }
        
        book.SetTitle(request.Title);
        book.SetAuthor(author);
        book.SetGenre(genre);

        await _booksRepository.UpdateAsync(book, cancellationToken);
        return book;
    }
}