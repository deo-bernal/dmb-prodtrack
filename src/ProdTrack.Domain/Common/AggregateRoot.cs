namespace ProdTrack.Domain.Common;

/// <summary>Aggregate root: consistency boundary, optimistic concurrency and domain events.</summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>SQL Server rowversion used for optimistic concurrency.</summary>
    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
