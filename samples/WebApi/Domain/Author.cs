namespace Domain;

/// <summary>
/// Books author.
/// </summary>
public class Author
{
    /// <summary>
    /// Max first and last name length.
    /// </summary>
    public const int MaxNameLength = 1000;
    
    /// <summary>
    /// Author id.
    /// </summary>
    public int AuthorId { get; private set; }
    
    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; private set; }
    
    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; private set; }

    private Author(){}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Author"/>.
    /// </summary>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    /// <exception cref="ArgumentException">Incorrect arguments.</exception>
    public Author(string firstName, string lastName)
    {
        UpdateFirstName(firstName);
        UpdateLastName(lastName);
    }

    /// <summary>
    /// Update first name.
    /// </summary>
    /// <param name="firstName">First name.</param>
    /// <exception cref="ArgumentException">Incorrect first name.</exception>
    public void UpdateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException($"{nameof(firstName)} is empty.", nameof(firstName));
        }

        if (firstName.Length > MaxNameLength)
        {
            throw new ArgumentException($"{nameof(firstName)} cannot exceed {MaxNameLength} characters.", nameof(firstName));
        }
        FirstName = firstName.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Update last name.
    /// </summary>
    /// <param name="lastName">Last name.</param>
    /// <exception cref="ArgumentException">Incorrect last name.</exception>
    public void UpdateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException($"{nameof(lastName)} is empty.", nameof(lastName));
        }
        if (lastName.Length > MaxNameLength)
        {
            throw new ArgumentException($"{nameof(lastName)} cannot exceed {MaxNameLength} characters.", nameof(lastName));
        }
        LastName = lastName.Trim().ToUpperInvariant();
    }
}