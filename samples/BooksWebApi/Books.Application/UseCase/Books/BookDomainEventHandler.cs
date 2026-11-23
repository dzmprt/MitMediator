using Books.Domain.Events;
using Microsoft.Extensions.Logging;
using MitMediator;

namespace Books.Application.UseCase.Books;

/// <summary>
/// Logs book domain event processing.
/// </summary>
internal sealed class BookDomainEventHandler(ILogger<BookDomainEventHandler> logger) :
    INotificationHandler<BookCreatedDomainEvent>,
    INotificationHandler<BookUpdatedDomainEvent>,
    INotificationHandler<BookDeletedDomainEvent>
{
    /// <inheritdoc/>
    public ValueTask HandleAsync(BookCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for book {BookId}.",
            nameof(BookCreatedDomainEvent),
            notification.Book.BookId);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(BookUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for book {BookId}.",
            nameof(BookUpdatedDomainEvent),
            notification.Book.BookId);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(BookDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {EventName} processed for book {BookId}.",
            nameof(BookDeletedDomainEvent),
            notification.BookId);
        return ValueTask.CompletedTask;
    }
}
