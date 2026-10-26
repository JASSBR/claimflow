---
name: add-domain-event-handler
description: "Implement and register an outbox domain event handler to process committed domain events."
---

> Evidence: value gate verified the facts below against this repository.

Implement and register an outbox domain event handler to process committed domain events.

Applies in: `claim-flow-building-blocks`, `claim-flow-claims`.

## What is specific to this repository

- Domain event handlers implement IDomainEventHandler<TEvent> with a HandleAsync method and must be idempotent because delivery is at-least-once. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L5-L5]
- Each module registers its own domain event types that contribute to the outbox using OutboxEventTypes. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs:L10-L11]
- The domain events interceptor throws an InvalidOperationException if an intercepted domain event type is not registered in the OutboxEventRegistry. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L58-L58]

## Rules this change touches

- [Invariant] All event types contributed to the OutboxEventRegistry must implement the IDomainEvent interface, or an ArgumentException is thrown. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs:L16-L25)
- [Invariant] The configuration argument passed to AddOutbox<TDbContext> must not be null, throwing ArgumentNullException if it is. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L18)
- [Invariant] Handlers implementing `IDomainEventHandler<in TEvent>` must be idempotent because domain event delivery via the outbox is at-least-once. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L5-L5)
- [InformalDecision] DomainEventsToOutboxInterceptor inherits from SaveChangesInterceptor to convert domain events buffered on tracked aggregates into OutboxMessage records right before SaveChanges commits, ensuring atomic persistence without dual writes. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L1-L13)
- [InformalDecision] Resolves outbox event types via an in-memory closed frozen dictionary keyed by type name rather than Type.GetType, preventing arbitrary type instantiation from tampered outbox rows. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs:L13-L15)

## Context

- DomainEventsToOutboxInterceptor is an Entity Framework Core SaveChangesInterceptor that converts buffered domain events on tracked aggregates into OutboxMessage records before SaveChanges commits, ensuring state changes and event notifications are committed atomically without dual writes. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L9-L14]
- AddOutbox registers the transactional outbox infrastructure for a specified DbContext type (TDbContext), configuring event types, outbox options, event interception, and background processing. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L16]
- ClaimsHub exposes route Path '/hubs/claims' and SignalR client method name ClaimChangedMethod 'claimChanged' for server-to-client notifications. [src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs:L8-L12]

## Start here

`src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs` [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L3-L10] is the worked example for this change, as it stands in the repository:

```csharp
namespace ClaimFlow.BuildingBlocks.Outbox;

/// <summary>Reacts to a domain event after it has been committed. Must be idempotent: delivery is at-least-once.</summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
```

## The same change elsewhere in this repository

- `src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs` [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L56-L56]: `if (!registry.IsRegistered(type))`
- `src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs` [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L58-L58]: `throw new InvalidOperationException($"Domain event {type.Name} is not registered in the outbox registry.");`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs:L11-L11]: `public sealed record OutboxEventTypes(IReadOnlyList<Type> Types);`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L14-L14]: `params Type[] eventTypes)`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L19-L19]: `services.AddSingleton(new OutboxEventTypes(eventTypes));`
- `src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs` [src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs:L20-L20]: `: IDomainEventHandler<ClaimDeclared>, IDomainEventHandler<ClaimStatusChanged>`
- `src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs` [src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs:L22-L22]: `public Task HandleAsync(ClaimDeclared domainEvent, CancellationToken cancellationToken) =>`
- `src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs` [src/Modules/Claims/ClaimFlow.Claims/Realtime/ClaimsHub.cs:L27-L27]: `public Task HandleAsync(ClaimStatusChanged domainEvent, CancellationToken cancellationToken) =>`

## Patterns

Required:
- `HandleAsync`
- `IDomainEvent`
- `IDomainEventHandler`
Forbidden:
- Avoid `INotificationHandler` in this change.

## Workflow

1. Define or locate the domain event type implementing IDomainEvent. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L7-L7]
2. Create a handler class implementing IDomainEventHandler<TEvent> where TEvent implements IDomainEvent. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L6-L6]
3. Implement IDomainEventHandler<TEvent> from ClaimFlow.BuildingBlocks.Outbox on the handler class. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L6-L6]
4. Implement HandleAsync(TEvent domainEvent, CancellationToken cancellationToken) to process the notification. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]
5. Implement HandleAsync ensuring operations are idempotent to tolerate at-least-once outbox deliveries. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]
6. Ensure HandleAsync operations are idempotent because outbox delivery guarantees at-least-once execution. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]

## Checklist

- [ ] Ensure the event type implements IDomainEvent. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L7-L7]
- [ ] Implement IDomainEventHandler<TEvent> with HandleAsync(TEvent domainEvent, CancellationToken cancellationToken). [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L6-L6]
- [ ] Ensure HandleAsync logic is idempotent because delivery is at-least-once. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]
- [ ] Include typeof(TEvent) in the eventTypes parameter of services.AddOutbox<TDbContext>. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L14-L14]
- [ ] Verify the handler class implements IDomainEventHandler<TEvent> for each targeted event. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L6-L6]
- [ ] Verify the domain event type is included in AddOutbox<ClaimsDbContext>(...) inside ClaimsModule. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L11]
- [ ] Verify HandleAsync accepts CancellationToken and completes asynchronously. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]
- [ ] Register TEvent in the module's AddOutbox call or OutboxEventTypes list. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L11]
- [ ] Accept and observe CancellationToken in HandleAsync. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]
- [ ] Event type is registered in OutboxEventRegistry via AddOutbox or OutboxEventTypes [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L12-L12]
- [ ] HandleAsync is idempotent to handle at-least-once delivery safely [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L9-L9]
- [ ] Verify that the event type is passed to AddOutbox or OutboxEventTypes registration. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L11]
