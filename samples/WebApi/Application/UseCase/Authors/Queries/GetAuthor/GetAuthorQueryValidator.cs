using FluentValidation;

namespace Application.UseCase.Authors.Queries.GetAuthor;

/// <summary>
/// Validator for <see cref="GetAuthorQuery"/>.
/// </summary>
internal sealed class GetAuthorQueryValidator : AbstractValidator<GetAuthorQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorQueryValidator"/>.
    /// </summary>
    public GetAuthorQueryValidator()
    {
        RuleFor(q => q.AuthorId).GreaterThan(0);
    }
}