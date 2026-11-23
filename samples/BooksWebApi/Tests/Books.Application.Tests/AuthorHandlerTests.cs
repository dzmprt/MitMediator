using Books.Application.Abstractions.Infrastructure;
using Books.Application.Exceptions;
using Books.Application.UseCase.Authors.Commands.CreateAuthor;
using Books.Application.UseCase.Authors.Commands.DeleteAuthor;
using Books.Application.UseCase.Authors.Commands.UpdateAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Books.Domain;
using Moq;
using System.Linq.Expressions;

namespace Books.Application.Tests;

public sealed class AuthorHandlerTests
{
    [Fact]
    public async Task CreateAuthor_AddsNormalizedEntity()
    {
        var repository = new Mock<IBaseRepository<Author>>();
        repository
            .Setup(x => x.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author entity, CancellationToken _) => entity);
        var handler = new CreateAuthorCommandHandler(repository.Object);

        var result = await handler.HandleAsync(
            new CreateAuthorCommand { FirstName = " Ada ", LastName = " Lovelace " },
            CancellationToken.None);

        Assert.Equal("ADA", result.FirstName);
        Assert.Equal("LOVELACE", result.LastName);
        repository.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAuthor_UpdatesExistingEntity()
    {
        var author = new Author("Ada", "Lovelace");
        var repository = CreateRepository(author);
        var handler = new UpdateAuthorCommandHandler(repository.Object);

        var result = await handler.HandleAsync(
            new UpdateAuthorCommand { AuthorId = 1, FirstName = "Grace", LastName = "Hopper" },
            CancellationToken.None);

        Assert.Same(author, result);
        Assert.Equal("GRACE", author.FirstName);
        repository.Verify(x => x.UpdateAsync(author, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAuthor_ThrowsWhenMissing()
    {
        var repository = CreateRepository(null);
        var handler = new UpdateAuthorCommandHandler(repository.Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(
                new UpdateAuthorCommand { AuthorId = 1, FirstName = "Grace", LastName = "Hopper" },
                CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAuthor_RemovesExistingEntity()
    {
        var author = new Author("Ada", "Lovelace");
        var repository = CreateRepository(author);
        var handler = new DeleteAuthorCommandHandler(repository.Object);

        await handler.HandleAsync(
            new DeleteAuthorCommand { AuthorId = 1 },
            CancellationToken.None);

        repository.Verify(x => x.RemoveAsync(author, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Single(author.DomainEvents.OfType<Books.Domain.Events.AuthorDeletedDomainEvent>());
    }

    [Fact]
    public async Task DeleteAuthor_ThrowsWhenMissing()
    {
        var handler = new DeleteAuthorCommandHandler(CreateRepository(null).Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(
                new DeleteAuthorCommand { AuthorId = 1 },
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAuthor_ReturnsOrThrows()
    {
        var author = new Author("Ada", "Lovelace");
        var foundHandler = new GetAuthorQueryHandler(CreateProvider(author).Object);
        var missingHandler = new GetAuthorQueryHandler(CreateProvider(null).Object);

        Assert.Same(
            author,
            await foundHandler.HandleAsync(
                new GetAuthorQuery { AuthorId = 1 },
                CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await missingHandler.HandleAsync(
                new GetAuthorQuery { AuthorId = 1 },
                CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ada ")]
    public async Task GetAuthorsByFilter_SupportsFilteredAndUnfilteredSearch(string? freeText)
    {
        var provider = new Mock<IBaseProvider<Author>>();
        var expected = Array.Empty<Author>();
        provider
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Author, bool>>?>(),
                It.IsAny<Expression<Func<Author, int>>>(),
                10,
                2,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Author[]>(expected));
        var handler = new GetAuthorsByFilterQueryHandler(provider.Object);

        var result = await handler.HandleAsync(
            new GetAuthorsByFilterQuery { FreeText = freeText, Limit = 10, Offset = 2 },
            CancellationToken.None);

        Assert.Same(expected, result);
    }

    private static Mock<IBaseRepository<Author>> CreateRepository(Author? value)
    {
        var repository = new Mock<IBaseRepository<Author>>();
        repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Author, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Author?>(value));
        repository
            .Setup(x => x.UpdateAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author entity, CancellationToken _) => entity);
        return repository;
    }

    private static Mock<IBaseProvider<Author>> CreateProvider(Author? value)
    {
        var provider = new Mock<IBaseProvider<Author>>();
        provider
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Author, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Author?>(value));
        return provider;
    }
}
