using System.Reflection;
using Books.Domain;
using Books.Domain.Events;
using Microsoft.EntityFrameworkCore;
using MitMediator;

namespace Books.Infrastructure;

/// <summary>
/// Application Db context.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Authors.
    /// </summary>
    public DbSet<Author> Authors { get; set; }

    /// <summary>
    /// Genres.
    /// </summary>
    public DbSet<Genre> Genres { get; set; }
    
    /// <summary>
    /// Books.
    /// </summary>
    public DbSet<Book> Books { get; set; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="options"><see cref="DbContextOptions"/>.</param>
    /// <param name="mediator">Mediator used to publish domain events.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    /// <inheritdoc/>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var domainEvents = CollectDomainEvents();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        ClearAndPublishDomainEvents(domainEvents);
        return result;
    }

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        await ClearAndPublishDomainEventsAsync(domainEvents);
        return result;
    }
    
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    private INotification[][] CollectDomainEvents()
    {
        return ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity.DomainEvents.ToArray())
            .ToArray();
    }

    private void ClearAndPublishDomainEvents(INotification[][] domainEvents)
    {
        ClearDomainEvents();
        foreach (var domainEvent in domainEvents.SelectMany(events => events))
        {
            PublishDomainEventAsync(domainEvent, CancellationToken.None)
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }
    }

    private async ValueTask ClearAndPublishDomainEventsAsync(INotification[][] domainEvents)
    {
        ClearDomainEvents();
        foreach (var domainEvent in domainEvents.SelectMany(events => events))
        {
            await PublishDomainEventAsync(domainEvent, CancellationToken.None);
        }
    }

    private ValueTask PublishDomainEventAsync(INotification domainEvent, CancellationToken cancellationToken)
    {
        return domainEvent switch
        {
            AuthorCreatedDomainEvent authorCreated => _mediator.PublishAsync(authorCreated, cancellationToken),
            AuthorUpdatedDomainEvent authorUpdated => _mediator.PublishAsync(authorUpdated, cancellationToken),
            AuthorDeletedDomainEvent authorDeleted => _mediator.PublishAsync(authorDeleted, cancellationToken),
            BookCreatedDomainEvent bookCreated => _mediator.PublishAsync(bookCreated, cancellationToken),
            BookUpdatedDomainEvent bookUpdated => _mediator.PublishAsync(bookUpdated, cancellationToken),
            BookDeletedDomainEvent bookDeleted => _mediator.PublishAsync(bookDeleted, cancellationToken),
            GenreCreatedDomainEvent genreCreated => _mediator.PublishAsync(genreCreated, cancellationToken),
            GenreDeletedDomainEvent genreDeleted => _mediator.PublishAsync(genreDeleted, cancellationToken),
            _ => ValueTask.FromException(new InvalidOperationException(
                $"Unsupported domain event type '{domainEvent.GetType().Name}'."))
        };
    }

    private void ClearDomainEvents()
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
}