using Books.Domain;
using MitMediator;

namespace Books.Application.UseCase.Authors.Queries.GetAuthorsByFilter;

/// <summary>
/// Get authors query.
/// </summary>
public struct GetAuthorsByFilterQuery : IRequest<Author[]>
{
    /// <summary>
    /// Limit.
    /// </summary>
    public int? Limit { get; init; }
    
    /// <summary>
    /// Offset.
    /// </summary>
    public int? Offset { get; init; }
    
    /// <summary>
    /// Free text.
    /// </summary>
    public string? FreeText { get; init; }
}