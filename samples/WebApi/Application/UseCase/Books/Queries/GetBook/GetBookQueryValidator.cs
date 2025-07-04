using FluentValidation;

namespace Application.UseCase.Books.Queries.GetBook;

/// <summary>
/// Validator for <see cref="GetBookQuery"/>.
/// </summary>
internal sealed class GetBookQueryValidator : AbstractValidator<GetBookQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookQueryValidator"/>.
    /// </summary>
    public GetBookQueryValidator()
    {
        RuleFor(q => q.BookId).GreaterThan(0);
    }
}