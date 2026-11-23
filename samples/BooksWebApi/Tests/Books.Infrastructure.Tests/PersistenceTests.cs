using System.Linq.Expressions;
using Books.Application.Abstractions.Infrastructure;
using Books.Domain;
using Books.Domain.Events;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MitMediator;
using Moq;
using System.Reflection;

namespace Books.Infrastructure.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public async Task Repository_CoversCrudAndProviderBranches()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var mediator = new Mock<IMediator>();
        await using var context = CreateContext(connection, mediator.Object);
        await context.Database.EnsureCreatedAsync(CancellationToken.None);
        var repository = new BaseRepository<Author>(context);
        var first = new Author("Ada", "Lovelace");
        var second = new Author("Grace", "Hopper");

        Assert.Same(first, await repository.AddAsync(first, CancellationToken.None));
        await repository.AddAsync(second, CancellationToken.None);

        Assert.Same(
            first,
            await repository.FirstOrDefaultAsync(
                author => author.FirstName == "ADA",
                CancellationToken.None));
        Assert.NotNull(await repository.FirstOrDefaultAsync(
            null,
            CancellationToken.None));
        Assert.Equal(2, (await repository.GetAllAsync(CancellationToken.None)).Length);
        Assert.True(await repository.AnyAsync(
            author => author.LastName == "HOPPER",
            CancellationToken.None));
        Assert.True(await repository.AnyAsync(null, CancellationToken.None));

        var filtered = await repository.SearchAsync<string>(
            author => author.LastName.Contains("E"),
            author => author.FirstName,
            1,
            0,
            CancellationToken.None);
        Assert.Single(filtered);

        var unfiltered = await repository.SearchAsync<string>(
            null,
            null,
            null,
            null,
            CancellationToken.None);
        Assert.Equal(2, unfiltered.Length);

        first.UpdateName("Augusta Ada", "Lovelace");
        Assert.Same(first, await repository.UpdateAsync(first, CancellationToken.None));
        Assert.Equal(
            "AUGUSTA ADA",
            (await repository.FirstOrDefaultAsync(
                author => author.LastName == "LOVELACE",
                CancellationToken.None))!.FirstName);

        await repository.RemoveAsync(second, CancellationToken.None);
        Assert.False(await repository.AnyAsync(
            author => author.LastName == "HOPPER",
            CancellationToken.None));
    }

    [Fact]
    public async Task DbContext_PublishesAndClearsEventsAfterAsyncAndSyncSave()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var mediator = new Mock<IMediator>();
        await using var context = CreateContext(connection, mediator.Object);
        await context.Database.EnsureCreatedAsync(CancellationToken.None);
        var first = new Author("Ada", "Lovelace");

        context.Authors.Add(first);
        await context.SaveChangesAsync(CancellationToken.None);

        mediator.Verify(
            x => x.PublishAsync(
                It.Is<AuthorCreatedDomainEvent>(domainEvent => domainEvent.Author == first),
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Empty(first.DomainEvents);

        var second = new Genre("Science");
        context.Genres.Add(second);
        context.SaveChanges();

        mediator.Verify(
            x => x.PublishAsync(
                It.Is<GenreCreatedDomainEvent>(domainEvent => domainEvent.Genre == second),
                CancellationToken.None),
            Times.Once);
        Assert.Empty(second.DomainEvents);
    }

    [Fact]
    public async Task DbContext_MapsAllAggregatesAndIgnoresDomainEvents()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        await using var context = CreateContext(connection, Mock.Of<IMediator>());

        Assert.Null(context.Model.FindEntityType(typeof(Author))!.FindProperty(nameof(Entity.DomainEvents)));
        Assert.Null(context.Model.FindEntityType(typeof(Book))!.FindProperty(nameof(Entity.DomainEvents)));
        Assert.Null(context.Model.FindEntityType(typeof(Genre))!.FindProperty(nameof(Entity.DomainEvents)));
        Assert.Equal(
            1000,
            context.Model.FindEntityType(typeof(Book))!.FindProperty(nameof(Book.Title))!.GetMaxLength());
    }

    [Fact]
    public async Task DbContext_DispatchesEveryDomainEventTypeAndRejectsUnknown()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var mediator = new Mock<IMediator>();
        await using var context = CreateContext(connection, mediator.Object);
        var author = new Author("Ada", "Lovelace");
        var genre = new Genre("Science");
        var book = new Book("Notes", author, genre);
        INotification[] events =
        [
            new AuthorCreatedDomainEvent(author),
            new AuthorUpdatedDomainEvent(author),
            new AuthorDeletedDomainEvent(1),
            new BookCreatedDomainEvent(book),
            new BookUpdatedDomainEvent(book),
            new BookDeletedDomainEvent(1),
            new GenreCreatedDomainEvent(genre),
            new GenreDeletedDomainEvent("SCIENCE")
        ];
        var method = typeof(ApplicationDbContext).GetMethod(
            "PublishDomainEventAsync",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var domainEvent in events)
        {
            await (ValueTask)method.Invoke(context, [domainEvent, CancellationToken.None])!;
        }

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            InvokeAsync(method, context, new UnknownNotification()));
        Assert.Contains("UnknownNotification", exception.Message);
    }

    [Fact]
    public async Task DbContext_DoesNotPublishWhenSaveFails()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var mediator = new Mock<IMediator>();
        await using var context = CreateContext(connection, mediator.Object);
        await context.Database.EnsureCreatedAsync(CancellationToken.None);
        var genre = new Genre("Science");
        context.Genres.Add(genre);
        await context.SaveChangesAsync(CancellationToken.None);
        mediator.Invocations.Clear();
        context.Entry(genre).State = EntityState.Detached;
        context.Genres.Add(new Genre("Science"));

        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await context.SaveChangesAsync(CancellationToken.None));
        mediator.Verify(x => x.PublishAsync(
            It.IsAny<GenreCreatedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DependencyInjection_RegistersPersistenceServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOnlyMitMediator();
        services.AddPersistenceServices();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<DbContext>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IBaseRepository<Author>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IBaseProvider<Author>>());
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection, IMediator mediator)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options, mediator);
    }

    private static Task InvokeAsync(MethodInfo method, ApplicationDbContext context, INotification notification)
    {
        return ((ValueTask)method.Invoke(context, [notification, CancellationToken.None])!).AsTask();
    }

    private sealed record UnknownNotification : INotification;
}
