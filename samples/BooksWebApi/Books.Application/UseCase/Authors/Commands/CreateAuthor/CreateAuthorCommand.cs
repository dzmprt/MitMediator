using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Authors.Commands.CreateAuthor;

/// <summary>
/// Create author command.
/// </summary>
public struct CreateAuthorCommand : IRequest<Author>
{
    /// <summary>
    /// New author first name.
    /// </summary>
    public string FirstName { get; init; }
    
    /// <summary>
    /// New author last name.
    /// </summary>
    public string LastName { get; init; }
}