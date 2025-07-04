using Domain;
using FluentValidation;

namespace Application.UseCase.Authors.Queries.GetAuthorsByFilter;

internal sealed class GetAuthorsByFilterQueryValidator : AbstractValidator<GetAuthorsByFilterQuery>
{
    public GetAuthorsByFilterQueryValidator()
    {
        RuleFor(q => q.Limit).GreaterThanOrEqualTo(0).When(q => q.Limit.HasValue);
        RuleFor(q => q.Offset).GreaterThanOrEqualTo(0).When(q => q.Offset.HasValue);
        RuleFor(q => q.FreeText).MaximumLength(Author.MaxNameLength);
    }
}