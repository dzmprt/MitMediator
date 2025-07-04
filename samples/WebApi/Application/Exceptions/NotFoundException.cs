namespace Application.Exceptions;

/// <summary>
/// Not found exception.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/>.
    /// </summary>
    public NotFoundException() : base("Not found")
    {
    }
}