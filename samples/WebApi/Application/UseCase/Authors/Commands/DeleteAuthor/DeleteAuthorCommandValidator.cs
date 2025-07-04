using FluentValidation;

namespace Application.UseCase.Authors.Commands.DeleteAuthor;

/// <summary>
/// Validator for <see cref="DeleteAuthorCommand"/>.
/// </summary>
internal sealed class DeleteAuthorCommandValidator : AbstractValidator<DeleteAuthorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAuthorCommandValidator"/>.
    /// </summary>
    public DeleteAuthorCommandValidator()
    {
        RuleFor(x => x.AuthorId).GreaterThan(0);
    }
}