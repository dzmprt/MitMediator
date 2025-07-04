namespace Domain;

/// <summary>
/// Book.
/// </summary>
public class Book
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
    public string Title { get; private set; }
    
    /// <summary>
    /// Author.
    /// </summary>
    public Author Author { get; private set; }
    
    /// <summary>
    /// Genre.
    /// </summary>
    public Genre Genre { get; private set; }

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
        SetTitle(title);
        SetAuthor(author);
        SetGenre(genre);
    }

    /// <summary>
    /// Set genre.
    /// </summary>
    /// <param name="genre">Genre.</param>
    public void SetGenre(Genre genre)
    {
        Genre = genre;
    }

    /// <summary>
    /// Set author.
    /// </summary>
    /// <param name="author">Author.</param>
    public void SetAuthor(Author author)
    {
        Author = author;
    }

    /// <summary>
    /// Set title.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <exception cref="ArgumentException">Incorrect title.</exception>
    public void SetTitle(string title)
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
}