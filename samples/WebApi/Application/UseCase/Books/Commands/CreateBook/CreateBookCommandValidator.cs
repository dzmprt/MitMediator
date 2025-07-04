using Domain;
using FluentValidation;

namespace Application.UseCase.Books.Commands.CreateBook;

/// <summary>
/// Validator for <see cref="CreateBookCommand"/>
/// </summary>
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(Book.MaxTitleLength);
        RuleFor(c => c.AuthorId).GreaterThan(0);
        RuleFor(c => c.GenreName).NotEmpty().MaximumLength(Genre.MaxGenreNameLength);
    }
}