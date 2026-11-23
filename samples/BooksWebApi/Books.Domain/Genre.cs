using Books.Domain.Events;

namespace Books.Domain;

/// <summary>
/// Book genre.
/// </summary>
public class Genre : Entity
{
    /// <summary>
    /// Max genre name length.
    /// </summary>
    public const int MaxGenreNameLength = 1000;
    
    /// <summary>
    /// Genre name.
    /// </summary>
    public string GenreName { get; private set; } = null!;

    private Genre(){}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Genre"/>.
    /// </summary>
    /// <param name="genreName">Genre name</param>
    /// <exception cref="ArgumentException">Incorrect name.</exception>
    public Genre(string genreName)
    {
        if (string.IsNullOrWhiteSpace(genreName))
        {
            throw new ArgumentException($"{nameof(genreName)} is empty.", nameof(genreName));
        }
        if (genreName.Length > MaxGenreNameLength)
        {
            throw new ArgumentException($"{nameof(genreName)} cannot exceed {MaxGenreNameLength} characters.", nameof(genreName));
        }
        GenreName = genreName.Trim().ToUpperInvariant();
        AddDomainEvent(new GenreCreatedDomainEvent(this));
    }

    /// <summary>
    /// Marks the genre as deleted.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new GenreDeletedDomainEvent(GenreName));
    }
}