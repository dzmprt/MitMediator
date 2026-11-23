using Books.Application.Abstractions.Infrastructure;
using Books.Application.Exceptions;
using Books.Application.UseCase.Genres;
using Books.Application.UseCase.Genres.Commands.CreateGenre;
using Books.Application.UseCase.Genres.Commands.DeleteGenre;
using Books.Application.UseCase.Genres.Queries.GetGenres;
using Books.Domain;
using Books.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;

namespace Books.Application.Tests;

public sealed class GenreAndEventHandlerTests
{
    [Fact]
    public async Task CreateGenre_AddsNewGenre()
    {
        var repository = new Mock<IBaseRepository<Genre>>();
        repository
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Genre, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<bool>(false));
        repository
            .Setup(x => x.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Genre entity, CancellationToken _) => entity);
        var handler = new CreateGenreCommandHandler(repository.Object);

        var result = await handler.HandleAsync(
            new CreateGenreCommand { GenreName = " science " },
            CancellationToken.None);

        Assert.Equal("SCIENCE", result.GenreName);
        repository.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateGenre_RejectsDuplicate()
    {
        var repository = new Mock<IBaseRepository<Genre>>();
        repository
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Genre, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<bool>(true));
        var handler = new CreateGenreCommandHandler(repository.Object);

        var exception = await Assert.ThrowsAsync<BadOperationException>(async () =>
            await handler.HandleAsync(
                new CreateGenreCommand { GenreName = "Science" },
                CancellationToken.None));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task DeleteGenre_RemovesExistingOrThrows()
    {
        var genre = new Genre("Science");
        var repository = new Mock<IBaseRepository<Genre>>();
        repository
            .SetupSequence(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Genre, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Genre?>(genre))
            .Returns(new ValueTask<Genre?>((Genre?)null));
        var handler = new DeleteGenreCommandHandler(repository.Object);

        await handler.HandleAsync(
            new DeleteGenreCommand { GenreName = "science" },
            CancellationToken.None);
        Assert.IsType<GenreDeletedDomainEvent>(Assert.Single(genre.DomainEvents, e => e is GenreDeletedDomainEvent));
        repository.Verify(x => x.RemoveAsync(genre, It.IsAny<CancellationToken>()), Times.Once);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(
                new DeleteGenreCommand { GenreName = "science" },
                CancellationToken.None));
    }

    [Fact]
    public async Task GetGenres_ReturnsAllGenres()
    {
        var expected = new[] { new Genre("Science") };
        var provider = new Mock<IBaseProvider<Genre>>();
        provider
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Genre[]>(expected));

        var result = await new GetGenresQueryHandler(provider.Object).HandleAsync(
            new GetGenresQuery(),
            CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GenreEventHandler_LogsBothEventTypes()
    {
        var handler = new GenreDomainEventHandler(NullLogger<GenreDomainEventHandler>.Instance);
        var genre = new Genre("Science");

        await handler.HandleAsync(new GenreCreatedDomainEvent(genre), CancellationToken.None);
        await handler.HandleAsync(new GenreDeletedDomainEvent("SCIENCE"), CancellationToken.None);
    }
}
