using Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Domain;
using FluentValidation;

namespace Application.UseCase.Books.Queries.GetBooksByFilter;

internal sealed class GetBooksByFilterQueryValidator : AbstractValidator<GetBooksByFilterQuery>
{
    public GetBooksByFilterQueryValidator()
    {
        RuleFor(q => q.Limit).GreaterThanOrEqualTo(0).When(q => q.Limit.HasValue);
        RuleFor(q => q.Offset).GreaterThanOrEqualTo(0).When(q => q.Offset.HasValue);
        RuleFor(q => q.FreeText).MaximumLength(1000);
    }
}