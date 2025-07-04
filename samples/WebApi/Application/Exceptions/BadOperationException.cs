namespace Application.Exceptions;

/// <summary>
/// Bad operation exception.
/// </summary>
public class BadOperationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadOperationException"/>.
    /// </summary>
    public BadOperationException(string? message) : base(message)
    {
    }
}