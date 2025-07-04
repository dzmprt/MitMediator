using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Domain;
using MitMediator;

namespace Application.UseCase.Books.Commands.CreateBook;

/// <summary>
/// Handler for <see cref="CreateBookCommand"/>
/// </summary>
public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Book>
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
    public CreateBookCommandHandler(
        IBaseRepository<Book> booksRepository, 
        IBaseRepository<Author> authorsRepository,
        IBaseRepository<Genre> genresRepository)
    {
        _booksRepository = booksRepository;
        _authorsRepository = authorsRepository;
        _genresRepository = genresRepository;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Book> HandleAsync(CreateBookCommand request, CancellationToken cancellationToken)
    {
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
        var book = new Book(request.Title, author, genre);
        await _booksRepository.AddAsync(book, cancellationToken);
        return book;
    }
}