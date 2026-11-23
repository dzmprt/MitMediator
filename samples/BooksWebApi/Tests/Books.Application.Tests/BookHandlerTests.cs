using Books.Application.Abstractions.Infrastructure;
using Books.Application.Exceptions;
using Books.Application.UseCase.Books.Commands.CreateBook;
using Books.Application.UseCase.Books.Commands.DeleteBook;
using Books.Application.UseCase.Books.Commands.UpdateBook;
using Books.Application.UseCase.Books.Queries.GetBook;
using Books.Application.UseCase.Books.Queries.GetBooksByFilter;
using Books.Domain;
using Moq;
using System.Linq.Expressions;

namespace Books.Application.Tests;

public sealed class BookHandlerTests
{
    [Fact]
    public async Task CreateBook_AddsBook()
    {
        var (handler, books, _, _) = CreateHandler(
            new Author("Ada", "Lovelace"),
            new Genre("Science"));

        var result = await handler.HandleAsync(
            new CreateBookCommand { Title = " Notes ", AuthorId = 1, GenreName = "science" },
            CancellationToken.None);

        Assert.Equal("NOTES", result.Title);
        books.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBook_RejectsMissingAuthor()
    {
        var (handler, _, _, _) = CreateHandler(null, new Genre("Science"));

        var exception = await Assert.ThrowsAsync<BadOperationException>(async () =>
            await handler.HandleAsync(
                new CreateBookCommand { Title = "Notes", AuthorId = 1, GenreName = "Science" },
                CancellationToken.None));

        Assert.Equal("Author not found", exception.Message);
    }

    [Fact]
    public async Task CreateBook_RejectsMissingGenre()
    {
        var (handler, _, _, _) = CreateHandler(new Author("Ada", "Lovelace"), null);

        var exception = await Assert.ThrowsAsync<BadOperationException>(async () =>
            await handler.HandleAsync(
                new CreateBookCommand { Title = "Notes", AuthorId = 1, GenreName = "Science" },
                CancellationToken.None));

        Assert.Equal("Genre not found", exception.Message);
    }

    [Fact]
    public async Task UpdateBook_UpdatesExistingBook()
    {
        var author = new Author("Ada", "Lovelace");
        var genre = new Genre("Science");
        var book = new Book("Notes", author, genre);
        var (handler, books, _, _) = CreateUpdateHandler(book, author, genre);

        var result = await handler.HandleAsync(
            new UpdateBookCommand
            {
                BookId = 1,
                Title = "Analytical Engine",
                AuthorId = 1,
                GenreName = "Science"
            },
            CancellationToken.None);

        Assert.Same(book, result);
        Assert.Equal("ANALYTICAL ENGINE", result.Title);
        books.Verify(x => x.UpdateAsync(book, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_ThrowsForEveryMissingEntity()
    {
        var author = new Author("Ada", "Lovelace");
        var genre = new Genre("Science");
        var book = new Book("Notes", author, genre);
        var scenarios = new[]
        {
            CreateUpdateHandler(null, author, genre).Handler,
            CreateUpdateHandler(book, null, genre).Handler,
            CreateUpdateHandler(book, author, null).Handler
        };

        foreach (var handler in scenarios)
        {
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await handler.HandleAsync(
                    new UpdateBookCommand
                    {
                        BookId = 1,
                        Title = "New",
                        AuthorId = 1,
                        GenreName = "Science"
                    },
                    CancellationToken.None));
        }
    }

    [Fact]
    public async Task DeleteBook_RemovesExistingOrThrows()
    {
        var book = new Book(
            "Notes",
            new Author("Ada", "Lovelace"),
            new Genre("Science"));
        var foundRepository = CreateBookRepository(book);
        var missingRepository = CreateBookRepository(null);

        await new DeleteBookCommandHandler(foundRepository.Object).HandleAsync(
            new DeleteBookCommand { BookId = 1 },
            CancellationToken.None);
        foundRepository.Verify(x => x.RemoveAsync(book, It.IsAny<CancellationToken>()), Times.Once);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await new DeleteBookCommandHandler(missingRepository.Object).HandleAsync(
                new DeleteBookCommand { BookId = 1 },
                CancellationToken.None));
    }

    [Fact]
    public async Task GetBook_ReturnsOrThrows()
    {
        var book = new Book(
            "Notes",
            new Author("Ada", "Lovelace"),
            new Genre("Science"));
        var found = new Mock<IBaseProvider<Book>>();
        var missing = new Mock<IBaseProvider<Book>>();
        SetupProvider(found, book);
        SetupProvider(missing, null);

        Assert.Same(
            book,
            await new GetBookQueryHandler(found.Object).HandleAsync(
                new GetBookQuery { BookId = 1 },
                CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await new GetBookQueryHandler(missing.Object).HandleAsync(
                new GetBookQuery { BookId = 1 },
                CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" science ")]
    public async Task GetBooksByFilter_SupportsBothPredicateBranches(string? freeText)
    {
        var expected = Array.Empty<Book>();
        var provider = new Mock<IBaseProvider<Book>>();
        provider
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Book, bool>>?>(),
                It.IsAny<Expression<Func<Book, int>>>(),
                5,
                1,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book[]>(expected));
        var handler = new GetBooksByFilterQueryHandler(provider.Object);

        var result = await handler.HandleAsync(
            new GetBooksByFilterQuery { FreeText = freeText, Limit = 5, Offset = 1 },
            CancellationToken.None);

        Assert.Same(expected, result);
    }

    private static (
        CreateBookCommandHandler Handler,
        Mock<IBaseRepository<Book>> Books,
        Mock<IBaseRepository<Author>> Authors,
        Mock<IBaseRepository<Genre>> Genres) CreateHandler(Author? author, Genre? genre)
    {
        var books = CreateBookRepository(null);
        books
            .Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book entity, CancellationToken _) => entity);
        var authors = CreateRepository(author);
        var genres = CreateRepository(genre);
        return (new CreateBookCommandHandler(books.Object, authors.Object, genres.Object), books, authors, genres);
    }

    private static (
        UpdateBookCommandHandler Handler,
        Mock<IBaseRepository<Book>> Books,
        Mock<IBaseRepository<Author>> Authors,
        Mock<IBaseRepository<Genre>> Genres) CreateUpdateHandler(Book? book, Author? author, Genre? genre)
    {
        var books = CreateBookRepository(book);
        books
            .Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book entity, CancellationToken _) => entity);
        var authors = CreateRepository(author);
        var genres = CreateRepository(genre);
        return (new UpdateBookCommandHandler(books.Object, authors.Object, genres.Object), books, authors, genres);
    }

    private static Mock<IBaseRepository<T>> CreateRepository<T>(T? value) where T : class
    {
        var repository = new Mock<IBaseRepository<T>>();
        repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<T, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<T?>(value));
        return repository;
    }

    private static Mock<IBaseRepository<Book>> CreateBookRepository(Book? value)
        => CreateRepository(value);

    private static void SetupProvider(Mock<IBaseProvider<Book>> provider, Book? book)
    {
        provider
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Book, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book?>(book));
    }
}
