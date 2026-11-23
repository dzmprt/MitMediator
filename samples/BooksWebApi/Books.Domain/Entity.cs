using System.Text.Json.Serialization;
using MitMediator;

namespace Books.Domain;

/// <summary>
/// Base type for domain entities that raise domain events.
/// </summary>
public abstract class Entity
{
    private readonly List<INotification> _domainEvents = [];

    /// <summary>
    /// Domain events waiting to be published.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity.
    /// </summary>
    protected void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes all pending domain events after publication.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
