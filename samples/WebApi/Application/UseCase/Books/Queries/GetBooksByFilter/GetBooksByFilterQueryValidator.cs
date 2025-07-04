using Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Domain;
using FluentValidation;

namespace Application.UseCase.Books.Queries.GetBooksByFilter;

/// <summary>
/// Validator for <see cref="GetBooksByFilterQuery"/>
/// </summary>
internal sealed class GetBooksByFilterQueryValidator : AbstractValidator<GetBooksByFilterQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksByFilterQueryValidator"/>.
    /// </summary>
    public GetBooksByFilterQueryValidator()
    {
        RuleFor(q => q.Limit).GreaterThanOrEqualTo(0).When(q => q.Limit.HasValue);
        RuleFor(q => q.Offset).GreaterThanOrEqualTo(0).When(q => q.Offset.HasValue);
        RuleFor(q => q.FreeText).MaximumLength(1000);
    }
}