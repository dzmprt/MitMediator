using Domain;
using FluentValidation;

namespace Application.UseCase.Authors.Commands.UpdateAuthor;

/// <summary>
/// Validator for <see cref="UpdateAuthorCommandValidator"/>.
/// </summary>
internal sealed class UpdateAuthorCommandValidator : AbstractValidator<UpdateAuthorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAuthorCommandValidator"/>.
    /// </summary>
    public UpdateAuthorCommandValidator()
    {
        RuleFor(author => author.FirstName).NotEmpty().MaximumLength(Author.MaxNameLength);
        RuleFor(author => author.LastName).NotEmpty().MaximumLength(Author.MaxNameLength);
    }
}