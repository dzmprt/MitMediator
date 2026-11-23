using Books.Domain.Events;
using Microsoft.Extensions.Logging;
using MitMediator;

namespace Books.Application.UseCase.Authors;

/// <summary>
/// Logs author domain event processing.
/// </summary>
internal sealed class AuthorDomainEventHandler(ILogger<AuthorDomainEventHandler> logger) :
    INotificationHandler<AuthorCreatedDomainEvent>,
    INotificationHandler<AuthorUpdatedDomainEvent>,
    INotificationHandler<AuthorDeletedDomainEvent>
{
    /// <inheritdoc/>
    public ValueTask HandleAsync(AuthorCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for author {AuthorId}.",
            nameof(AuthorCreatedDomainEvent),
            notification.Author.AuthorId);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(AuthorUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for author {AuthorId}.",
            nameof(AuthorUpdatedDomainEvent),
            notification.Author.AuthorId);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(AuthorDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for author {AuthorId}.",
            nameof(AuthorDeletedDomainEvent),
            notification.AuthorId);
        return ValueTask.CompletedTask;
    }
}
