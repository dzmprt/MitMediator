using Books.Domain.Events;

namespace Books.Domain;

/// <summary>
/// Books author.
/// </summary>
public class Author : Entity
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
    public string FirstName { get; private set; } = null!;
    
    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; private set; } = null!;

    private Author(){}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Author"/>.
    /// </summary>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    /// <exception cref="ArgumentException">Incorrect arguments.</exception>
    public Author(string firstName, string lastName)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        AddDomainEvent(new AuthorCreatedDomainEvent(this));
    }

    /// <summary>
    /// Updates the author's name.
    /// </summary>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    public void UpdateName(string firstName, string lastName)
    {
        var previousFirstName = FirstName;
        var previousLastName = LastName;

        SetFirstName(firstName);
        SetLastName(lastName);

        if (previousFirstName != FirstName || previousLastName != LastName)
        {
            AddDomainEvent(new AuthorUpdatedDomainEvent(this));
        }
    }

    /// <summary>
    /// Update first name.
    /// </summary>
    /// <param name="firstName">First name.</param>
    /// <exception cref="ArgumentException">Incorrect first name.</exception>
    public void UpdateFirstName(string firstName)
    {
        var previousFirstName = FirstName;
        SetFirstName(firstName);

        if (previousFirstName != FirstName)
        {
            AddDomainEvent(new AuthorUpdatedDomainEvent(this));
        }
    }

    private void SetFirstName(string firstName)
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
        var previousLastName = LastName;
        SetLastName(lastName);

        if (previousLastName != LastName)
        {
            AddDomainEvent(new AuthorUpdatedDomainEvent(this));
        }
    }

    /// <summary>
    /// Marks the author as deleted.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new AuthorDeletedDomainEvent(AuthorId));
    }

    private void SetLastName(string lastName)
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