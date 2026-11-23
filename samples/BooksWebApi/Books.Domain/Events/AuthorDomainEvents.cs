using MitMediator;

namespace Books.Domain.Events;

/// <summary>
/// Raised after an author is created.
/// </summary>
public sealed record AuthorCreatedDomainEvent(Author Author) : INotification;

/// <summary>
/// Raised after an author is updated.
/// </summary>
public sealed record AuthorUpdatedDomainEvent(Author Author) : INotification;

/// <summary>
/// Raised after an author is deleted.
/// </summary>
public sealed record AuthorDeletedDomainEvent(int AuthorId) : INotification;
