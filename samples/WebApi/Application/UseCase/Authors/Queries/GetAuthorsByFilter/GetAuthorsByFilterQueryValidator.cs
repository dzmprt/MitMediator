using Domain;
using FluentValidation;

namespace Application.UseCase.Authors.Queries.GetAuthorsByFilter;

/// <summary>
/// Validator for <see cref="GetAuthorsByFilterQuery"/>.
/// </summary>
internal sealed class GetAuthorsByFilterQueryValidator : AbstractValidator<GetAuthorsByFilterQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorsByFilterQueryValidator"/>.
    /// </summary>
    public GetAuthorsByFilterQueryValidator()
    {
        RuleFor(q => q.Limit).GreaterThanOrEqualTo(0).When(q => q.Limit.HasValue);
        RuleFor(q => q.Offset).GreaterThanOrEqualTo(0).When(q => q.Offset.HasValue);
        RuleFor(q => q.FreeText).MaximumLength(Author.MaxNameLength);
    }
}