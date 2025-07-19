using Books.Application.UseCase.Authors.Commands.CreateAuthor;
using Books.Domain;
using FluentValidation;

namespace Books.Application.UseCase.Genres.Commands.CreateGenre;

/// <summary>
/// Validator for <see cref="CreateGenreCommand"/>.
/// </summary>
internal sealed class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGenreCommandValidator"/>.
    /// </summary>
    public CreateGenreCommandValidator()
    {
        RuleFor(author => author.GenreName).NotEmpty().MaximumLength(Genre.MaxGenreNameLength);
    }
}