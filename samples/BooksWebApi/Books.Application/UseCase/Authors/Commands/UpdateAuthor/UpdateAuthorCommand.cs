using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Authors.Commands.UpdateAuthor;

/// <summary>
/// Update author.
/// </summary>
public struct UpdateAuthorCommand : IRequest<Author>
{
    /// <summary>
    /// Author id.
    /// </summary>
    public int AuthorId { get; init; }
    
    /// <summary>
    /// First name.
    /// </summary>
    public required string FirstName { get; init; }
    
    /// <summary>
    /// Last name.
    /// </summary>
    public required string LastName { get; init; }
}