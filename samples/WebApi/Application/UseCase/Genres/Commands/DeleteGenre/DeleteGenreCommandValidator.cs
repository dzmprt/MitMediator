using Domain;
using FluentValidation;

namespace Application.UseCase.Genres.Commands.DeleteGenre;

/// <summary>
/// Validator for <see cref="DeleteGenreCommand"/>.
/// </summary>
internal sealed class DeleteGenreCommandValidator : AbstractValidator<DeleteGenreCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteGenreCommandValidator"/>.
    /// </summary>
    public DeleteGenreCommandValidator()
    {
        RuleFor(x => x.GenreName).NotEmpty().MaximumLength(Genre.MaxGenreNameLength);
    }
}