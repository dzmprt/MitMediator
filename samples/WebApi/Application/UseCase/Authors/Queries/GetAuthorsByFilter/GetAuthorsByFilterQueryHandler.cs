using Application.Abstractions.Infrastructure;
using Domain;
using MitMediator;

namespace Application.UseCase.Authors.Queries.GetAuthorsByFilter;

/// <summary>
/// Handler for <see cref="GetAuthorsByFilterQuery"/>.
/// </summary>
internal sealed class GetAuthorsByFilterQueryHandler : IRequestHandler<GetAuthorsByFilterQuery, Author[]>
{
    private readonly IBaseProvider<Author> _authorProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorsByFilterQueryHandler"/>.
    /// </summary>
    /// <param name="authorProvider">Author provider.</param>
    public GetAuthorsByFilterQueryHandler(IBaseProvider<Author> authorProvider)
    {
        _authorProvider = authorProvider;
    }
    
    /// <inheritdoc/>
    public ValueTask<Author[]> HandleAsync(GetAuthorsByFilterQuery request, CancellationToken cancellationToken)
    {
        var freeText = request.FreeText?.Trim().ToUpperInvariant();
        return _authorProvider.SearchAsync(
            freeText is null
                ? null
                : q => q.FirstName.Contains(freeText) || q.LastName.Contains(freeText),
            o => o.AuthorId,
            request.Limit,
            request.Offset,
            cancellationToken
        );
    }
}