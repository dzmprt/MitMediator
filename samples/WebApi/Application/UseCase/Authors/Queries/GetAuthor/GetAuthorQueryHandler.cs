using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Domain;
using MitMediator;

namespace Application.UseCase.Authors.Queries.GetAuthor;

/// <summary>
/// Handler for <see cref="GetAuthorQuery"/>.
/// </summary>
internal sealed class GetAuthorQueryHandler : IRequestHandler<GetAuthorQuery, Author>
{
    private readonly IBaseProvider<Author> _authorProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorQueryHandler"/>.
    /// </summary>
    /// <param name="authorProvider">Author provider.</param>
    public GetAuthorQueryHandler(IBaseProvider<Author> authorProvider)
    {
        _authorProvider = authorProvider;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Author> HandleAsync(GetAuthorQuery query, CancellationToken cancellationToken)
    {
        var author = await _authorProvider.FirstOrDefaultAsync(q => q.AuthorId == query.AuthorId, cancellationToken);
        if (author is null)
        {
            throw new NotFoundException();
        }

        return author;
    }
}