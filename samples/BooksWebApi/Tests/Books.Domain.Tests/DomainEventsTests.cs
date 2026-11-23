using Books.Domain;
using Books.Domain.Events;
using System.Reflection;

namespace Books.Domain.Tests;

public sealed class DomainEventsTests
{
    [Fact]
    public void EfMaterializationConstructors_CanCreateEntities()
    {
        Assert.NotNull(Activator.CreateInstance(typeof(Author), nonPublic: true));
        Assert.NotNull(Activator.CreateInstance(typeof(Book), nonPublic: true));
        Assert.NotNull(Activator.CreateInstance(typeof(Genre), nonPublic: true));
    }

    [Fact]
    public void Author_RaisesCreatedUpdatedAndDeletedEvents()
    {
        var author = new Author("Ada", "Lovelace");
        Assert.IsType<AuthorCreatedDomainEvent>(Assert.Single(author.DomainEvents));

        author.ClearDomainEvents();
        author.UpdateName("Augusta Ada", "Lovelace");
        Assert.IsType<AuthorUpdatedDomainEvent>(Assert.Single(author.DomainEvents));
        Assert.Equal("AUGUSTA ADA", author.FirstName);

        author.ClearDomainEvents();
        author.MarkDeleted();
        Assert.IsType<AuthorDeletedDomainEvent>(Assert.Single(author.DomainEvents));
    }

    [Fact]
    public void Author_UpdateMethodsRaiseOnlyForChanges()
    {
        var author = new Author("Ada", "Lovelace");
        author.ClearDomainEvents();

        author.UpdateName(" Ada ", "lovelace");
        Assert.Empty(author.DomainEvents);

        author.UpdateFirstName(" Ada ");
        author.UpdateLastName(" lovelace ");
        Assert.Empty(author.DomainEvents);

        author.UpdateFirstName("Grace");
        author.UpdateLastName("Hopper");
        Assert.Collection(
            author.DomainEvents,
            item => Assert.IsType<AuthorUpdatedDomainEvent>(item),
            item => Assert.IsType<AuthorUpdatedDomainEvent>(item));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Author_RejectsEmptyNames(string value)
    {
        Assert.Throws<ArgumentException>(() => new Author(value, "Valid"));
        Assert.Throws<ArgumentException>(() => new Author("Valid", value));
    }

    [Fact]
    public void Author_RejectsNamesOverMaximumLength()
    {
        var tooLong = new string('A', Author.MaxNameLength + 1);
        Assert.Throws<ArgumentException>(() => new Author(tooLong, "Valid"));
        Assert.Throws<ArgumentException>(() => new Author("Valid", tooLong));
    }

    [Fact]
    public void Book_RaisesAndDeduplicatesEvents()
    {
        var author = new Author("Ada", "Lovelace");
        var otherAuthor = new Author("Grace", "Hopper");
        var genre = new Genre("Science Fiction");
        var otherGenre = new Genre("Biography");
        var book = new Book("Notes", author, genre);

        Assert.IsType<BookCreatedDomainEvent>(Assert.Single(book.DomainEvents));

        book.ClearDomainEvents();
        book.Update(" notes ", author, genre);
        Assert.Empty(book.DomainEvents);

        book.Update("notes", otherAuthor, genre);
        Assert.Single(book.DomainEvents);
        book.ClearDomainEvents();
        book.Update("notes", otherAuthor, otherGenre);
        Assert.Single(book.DomainEvents);
        book.ClearDomainEvents();

        book.SetTitle(" notes ");
        book.SetAuthor(otherAuthor);
        book.SetGenre(otherGenre);
        Assert.Empty(book.DomainEvents);

        book.SetTitle("Analytical Engine");
        book.SetAuthor(author);
        book.SetGenre(genre);
        Assert.Equal(3, book.DomainEvents.Count);

        book.ClearDomainEvents();
        book.Update("Compiler", author, genre);
        Assert.IsType<BookUpdatedDomainEvent>(Assert.Single(book.DomainEvents));

        book.ClearDomainEvents();
        book.MarkDeleted();
        Assert.IsType<BookDeletedDomainEvent>(Assert.Single(book.DomainEvents));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Book_RejectsEmptyTitle(string title)
    {
        Assert.Throws<ArgumentException>(
            () => new Book(title, new Author("Ada", "Lovelace"), new Genre("Science")));
    }

    [Fact]
    public void Book_RejectsTitleOverMaximumLength()
    {
        Assert.Throws<ArgumentException>(
            () => new Book(
                new string('A', Book.MaxTitleLength + 1),
                new Author("Ada", "Lovelace"),
                new Genre("Science")));
    }

    [Fact]
    public void Genre_RaisesCreatedAndDeletedEvents()
    {
        var genre = new Genre("Fantasy");
        Assert.IsType<GenreCreatedDomainEvent>(Assert.Single(genre.DomainEvents));

        genre.ClearDomainEvents();
        genre.MarkDeleted();
        var deletedEvent = Assert.IsType<GenreDeletedDomainEvent>(Assert.Single(genre.DomainEvents));
        Assert.Equal("FANTASY", deletedEvent.GenreName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Genre_RejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() => new Genre(name));
    }

    [Fact]
    public void Genre_RejectsNameOverMaximumLength()
    {
        Assert.Throws<ArgumentException>(() => new Genre(new string('A', Genre.MaxGenreNameLength + 1)));
    }
}
