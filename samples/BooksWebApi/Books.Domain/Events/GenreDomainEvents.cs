using MitMediator;

namespace Books.Domain.Events;

/// <summary>
/// Raised after a genre is created.
/// </summary>
public sealed record GenreCreatedDomainEvent(Genre Genre) : INotification;

/// <summary>
/// Raised after a genre is deleted.
/// </summary>
public sealed record GenreDeletedDomainEvent(string GenreName) : INotification;
