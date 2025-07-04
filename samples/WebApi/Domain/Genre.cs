namespace Domain;

/// <summary>
/// Book genre.
/// </summary>
public class Genre
{
    /// <summary>
    /// Max genre name length.
    /// </summary>
    public const int MaxGenreNameLength = 1000;
    
    /// <summary>
    /// Genre name.
    /// </summary>
    public string GenreName { get; private set; }

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
    }
}