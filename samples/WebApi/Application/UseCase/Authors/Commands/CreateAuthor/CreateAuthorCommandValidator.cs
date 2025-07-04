using Domain;
using FluentValidation;

namespace Application.UseCase.Authors.Commands.CreateAuthor;

/// <summary>
/// Validator for <see cref="CreateAuthorCommand"/>.
/// </summary>
internal sealed class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAuthorCommandValidator"/>.
    /// </summary>
    public CreateAuthorCommandValidator()
    {
        RuleFor(author => author.FirstName).NotEmpty().MaximumLength(Author.MaxNameLength);
        RuleFor(author => author.LastName).NotEmpty().MaximumLength(Author.MaxNameLength);
    }
}