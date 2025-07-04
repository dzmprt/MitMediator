using Domain;
using FluentValidation;

namespace Application.UseCase.Books.Commands.CreateBook;

/// <summary>
/// Validator for <see cref="CreateBookCommand"/>
/// </summary>
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBookCommandValidator"/>.
    /// </summary>
    public CreateBookCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(Book.MaxTitleLength);
        RuleFor(c => c.AuthorId).GreaterThan(0);
        RuleFor(c => c.GenreName).NotEmpty().MaximumLength(Genre.MaxGenreNameLength);
    }
}