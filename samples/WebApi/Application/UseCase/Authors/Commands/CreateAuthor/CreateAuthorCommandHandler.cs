using Application.Abstractions.Infrastructure;
using Domain;
using MitMediator;

namespace Application.UseCase.Authors.Commands.CreateAuthor;

/// <summary>
/// Handler for <see cref="CreateAuthorCommand"/>.
/// </summary>
internal sealed class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Author>
{
    private readonly IBaseRepository<Author> _authorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAuthorCommandHandler"/>.
    /// </summary>
    /// <param name="authorRepository">Author repository.</param>
    public CreateAuthorCommandHandler(IBaseRepository<Author> authorRepository)
    {
        _authorRepository = authorRepository;
    }
    
    /// <inheritdoc/>
    public async ValueTask<Author> HandleAsync(CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = new Author(command.FirstName, command.LastName);
        await _authorRepository.AddAsync(author, cancellationToken);
        return author;
    }
}