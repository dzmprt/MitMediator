using Application.UseCase.Books.Commands.CreateBook;
using Domain;
using FluentValidation;

namespace Application.UseCase.Books.Commands.UpdateBook;

/// <summary>
/// Validator for <see cref="CreateBookCommand"/>
/// </summary>
public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCommandValidator"/>.
    /// </summary>
    public UpdateBookCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(Book.MaxTitleLength);
        RuleFor(c => c.AuthorId).GreaterThan(0);
        RuleFor(c => c.GenreName).NotEmpty().MaximumLength(Genre.MaxGenreNameLength);
    }
}