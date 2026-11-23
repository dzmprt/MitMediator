using MitMediator;

namespace Books.Domain.Events;

/// <summary>
/// Raised after a book is created.
/// </summary>
public sealed record BookCreatedDomainEvent(Book Book) : INotification;

/// <summary>
/// Raised after a book is updated.
/// </summary>
public sealed record BookUpdatedDomainEvent(Book Book) : INotification;

/// <summary>
/// Raised after a book is deleted.
/// </summary>
public sealed record BookDeletedDomainEvent(int BookId) : INotification;
