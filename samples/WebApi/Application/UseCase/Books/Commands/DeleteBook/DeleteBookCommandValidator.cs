using Application.UseCase.Authors.Commands.DeleteAuthor;
using FluentValidation;

namespace Application.UseCase.Books.Commands.DeleteBook;

/// <summary>
/// Validator for <see cref="DeleteBookCommand"/>.
/// </summary>
internal sealed class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookCommandValidator"/>.
    /// </summary>
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
    }
}