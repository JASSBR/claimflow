---
name: register-domain-event
description: "Define a domain event implementing IDomainEvent with an occurrence timestamp for aggregate state changes."
---

> Evidence: value gate verified the facts below against this repository.

Define a domain event implementing IDomainEvent with an occurrence timestamp for aggregate state changes.

Applies in: `claim-flow-shared-kernel`.

## What is specific to this repository

- Domain events in ClaimFlow must implement the IDomainEvent interface, which defines an OccurredAt timestamp property of type DateTimeOffset. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L6]
- Aggregate roots inherit from AggregateRoot<TId> and record domain events using a protected Raise method that appends to an internal collection. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L30-L30]
- Aggregate roots implement IHasDomainEvents, exposing a read-only collection of buffered domain events and a method to clear them. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L8-L13]

## Rules this change touches

- [Invariant] The internal _domainEvents list is encapsulated and exposed externally only as a read-only collection via DomainEvents.AsReadOnly(), preventing external mutation without going through Raise or ClearDomainEvents. (approved, conviction 1.00, src/ClaimFlow.SharedKernel/AggregateRoot.cs:L21-L24)

## Start here

`src/ClaimFlow.SharedKernel/AggregateRoot.cs` [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L1-L31] is the worked example for this change, as it stands in the repository:

```csharp
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
```

## Workflow

1. Create a domain event type that implements IDomainEvent. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]
2. Add an OccurredAt property typed as DateTimeOffset to capture when the event happened. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L5-L5]
3. Dispatch the event from an aggregate root by calling Raise with the IDomainEvent instance. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]
4. Define the domain event record or class implementing IDomainEvent with OccurredAt. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]
5. Raise the event inside aggregate root business methods using Raise(domainEvent). [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L30-L30]

## Checklist

- [ ] Verify the event implements IDomainEvent. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]
- [ ] Verify OccurredAt is exposed as a DateTimeOffset property. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L5-L5]
- [ ] Verify the event is emitted by calling Raise inside an AggregateRoot<TId>. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L19-L19]
