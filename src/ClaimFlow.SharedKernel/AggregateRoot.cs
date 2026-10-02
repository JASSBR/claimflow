namespace ClaimFlow.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

/// <summary>
/// Consistency boundary: the only object of a cluster that outside code may hold a reference to.
/// Events are buffered here and turned into outbox rows inside the same transaction as the state change.
/// </summary>
public abstract class AggregateRoot<TId> : IHasDomainEvents
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected init; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
