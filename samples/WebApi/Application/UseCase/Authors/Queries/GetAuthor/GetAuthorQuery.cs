using Domain;
using MitMediator;

namespace Application.UseCase.Authors.Queries.GetAuthor;

/// <summary>
/// Get author query.
/// </summary>
public struct GetAuthorQuery : IRequest<Author>
{
    /// <summary>
    /// Author id.
    /// </summary>
    public int AuthorId { get; init; }
}