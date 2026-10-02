using ClaimFlow.SharedKernel;

namespace ClaimFlow.BuildingBlocks.Outbox;

/// <summary>Reacts to a domain event after it has been committed. Must be idempotent: delivery is at-least-once.</summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
