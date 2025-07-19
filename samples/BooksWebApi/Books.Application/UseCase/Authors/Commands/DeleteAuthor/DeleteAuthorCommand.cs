using MitMediator;

namespace Books.Application.UseCase.Authors.Commands.DeleteAuthor;

/// <summary>
/// Delete author command.
/// </summary>
public struct DeleteAuthorCommand : IRequest
{
    /// <summary>
    /// Author id.
    /// </summary>
    public int AuthorId { get; init; }
}