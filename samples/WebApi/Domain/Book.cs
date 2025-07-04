namespace Domain;

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
    public Book(string title, Author author, Genre genre)
    {
        SetTitle(title);
        SetAuthor(author);
        SetGenre(genre);
    }

    public void SetGenre(Genre genre)
    {
        Genre = genre;
    }

    public void SetAuthor(Author author)
    {
        Author = author;
    }

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