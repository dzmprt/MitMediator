using Application.Abstractions.Infrastructure;
using Application.Exceptions;
using Domain;
using MitMediator;

namespace Application.UseCase.Authors.Commands.UpdateAuthor;

/// <summary>
/// Handler for <see cref="UpdateAuthorCommand"/>.
/// </summary>
internal sealed class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Author>
{
    private readonly IBaseRepository<Author> _authorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAuthorCommandHandler"/>.
    /// </summary>
    /// <param name="authorRepository">Author repository.</param>
    public UpdateAuthorCommandHandler(IBaseRepository<Author> authorRepository)
    {
        _authorRepository = authorRepository;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Author> HandleAsync(UpdateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = await _authorRepository.FirstOrDefaultAsync(q => q.AuthorId == command.AuthorId, cancellationToken);
        if (author is null)
        {
            throw new NotFoundException();
        }
        author.UpdateFirstName(command.FirstName);
        author.UpdateLastName(command.LastName);
        await _authorRepository.UpdateAsync(author, cancellationToken);
        return author;
    }
}