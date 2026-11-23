using Books.Domain.Events;

namespace Books.Domain;

/// <summary>
/// Book.
/// </summary>
public class Book : Entity
{
    /// <summary>
    /// Max title length.
    /// </summary>
    public const int MaxTitleLength = 1000;
    
    /// <summary>
    /// Book id.
    /// </summary>
    public int BookId { get; private set; }
    
    /// <summary>
    /// Title.
    /// </summary>
    public string Title { get; private set; } = null!;
    
    /// <summary>
    /// Author.
    /// </summary>
    public Author Author { get; private set; } = null!;
    
    /// <summary>
    /// Genre.
    /// </summary>
    public Genre Genre { get; private set; } = null!;

    private Book(){}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Book"/>.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <param name="author">Author.</param>
    /// <param name="genre">Genre.</param>
    /// <exception cref="ArgumentException">Incorrect title.</exception>
    public Book(string title, Author author, Genre genre)
    {
        SetTitleCore(title);
        SetAuthorCore(author);
        SetGenreCore(genre);
        AddDomainEvent(new BookCreatedDomainEvent(this));
    }

    /// <summary>
    /// Updates the book details.
    /// </summary>
    public void Update(string title, Author author, Genre genre)
    {
        var previousTitle = Title;
        var previousAuthor = Author;
        var previousGenre = Genre;

        SetTitleCore(title);
        SetAuthorCore(author);
        SetGenreCore(genre);

        if (previousTitle != Title || !ReferenceEquals(previousAuthor, Author) || !ReferenceEquals(previousGenre, Genre))
        {
            AddDomainEvent(new BookUpdatedDomainEvent(this));
        }
    }

    /// <summary>
    /// Set genre.
    /// </summary>
    /// <param name="genre">Genre.</param>
    public void SetGenre(Genre genre)
    {
        var previousGenre = Genre;
        SetGenreCore(genre);

        if (!ReferenceEquals(previousGenre, Genre))
        {
            AddDomainEvent(new BookUpdatedDomainEvent(this));
        }
    }

    /// <summary>
    /// Set author.
    /// </summary>
    /// <param name="author">Author.</param>
    public void SetAuthor(Author author)
    {
        var previousAuthor = Author;
        SetAuthorCore(author);

        if (!ReferenceEquals(previousAuthor, Author))
        {
            AddDomainEvent(new BookUpdatedDomainEvent(this));
        }
    }

    /// <summary>
    /// Set title.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <exception cref="ArgumentException">Incorrect title.</exception>
    public void SetTitle(string title)
    {
        var previousTitle = Title;
        SetTitleCore(title);

        if (previousTitle != Title)
        {
            AddDomainEvent(new BookUpdatedDomainEvent(this));
        }
    }

    /// <summary>
    /// Marks the book as deleted.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new BookDeletedDomainEvent(BookId));
    }

    private void SetTitleCore(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException($"{nameof(title)} is empty.", nameof(title));
        }

        if (title.Length > MaxTitleLength)
        {
            throw new ArgumentException($"{nameof(title)} cannot exceed {MaxTitleLength} characters.", nameof(title));
        }
        Title = title.Trim().ToUpperInvariant();
    }

    private void SetAuthorCore(Author author)
    {
        Author = author;
    }

    private void SetGenreCore(Genre genre)
    {
        Genre = genre;
    }
}