using Books.Domain.Events;
using Microsoft.Extensions.Logging;
using MitMediator;

namespace Books.Application.UseCase.Genres;

/// <summary>
/// Logs genre domain event processing.
/// </summary>
internal sealed class GenreDomainEventHandler(ILogger<GenreDomainEventHandler> logger) :
    INotificationHandler<GenreCreatedDomainEvent>,
    INotificationHandler<GenreDeletedDomainEvent>
{
    /// <inheritdoc/>
    public ValueTask HandleAsync(GenreCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for genre {GenreName}.",
            nameof(GenreCreatedDomainEvent),
            notification.Genre.GenreName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(GenreDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for genre {GenreName}.",
            nameof(GenreDeletedDomainEvent),
            notification.GenreName);
        return ValueTask.CompletedTask;
    }
}
