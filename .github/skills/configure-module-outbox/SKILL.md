---
name: configure-module-outbox
description: "Configure a module DbContext and service collection to persist and dispatch outbox messages for domain events."
---

> Evidence: value gate verified the facts below against this repository.

Configure a module DbContext and service collection to persist and dispatch outbox messages for domain events.

Applies in: `claim-flow-building-blocks`.

## What is specific to this repository

- OutboxModelBuilderExtensions maps the OutboxMessage entity to the table outbox_messages. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L14-L14]
- OutboxModelBuilderExtensions creates a partial index on OccurredAt filtered where ProcessedAt IS NULL. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L20-L20]
- DomainEventsToOutboxInterceptor verifies that each domain event type is registered with OutboxEventRegistry, throwing an InvalidOperationException if it is not. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L56-L59]
- OutboxOptions binds to the configuration section named Outbox. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L24-L24]

## Rules this change touches

- [Invariant] The modelBuilder parameter passed to ApplyOutbox must not be null; an ArgumentNullException is thrown if it is null. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L8-L10)
- [Invariant] The configuration argument passed to AddOutbox<TDbContext> must not be null, throwing ArgumentNullException if it is. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L18)
- [Invariant] Upon successful dispatch and execution of domain event handlers for an outbox message, ProcessedAt must be set to the current UTC timestamp and LastError must be set to null. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L128-L131)
- [BusinessRule] When outbox message dispatch fails with an exception not caused by stopping cancellation, the message's Attempts counter is incremented by 1 and LastError is set to the exception message truncated to a maximum of 2000 characters. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L133-L141)
- [InformalDecision] DomainEventsToOutboxInterceptor inherits from SaveChangesInterceptor to convert domain events buffered on tracked aggregates into OutboxMessage records right before SaveChanges commits, ensuring atomic persistence without dual writes. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L1-L13)
- [InformalDecision] OutboxOptions configures outbox processing with default values of a 2-second PollingInterval and a BatchSize of 50 under configuration section name 'Outbox'. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L23-L29)

## Context

- AddOutbox registers the transactional outbox infrastructure for a specified DbContext type (TDbContext), configuring event types, outbox options, event interception, and background processing. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L16]
- ExecuteAsync runs a background loop using a PeriodicTimer with the configured PollingInterval and TimeProvider, calling ProcessBatchAsync on each tick and catching exceptions when cancellation has not been requested so that batch failures do not terminate the host service. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L69-L84]

## Start here

`src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs` [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L9-L48] is the worked example for this change, as it stands in the repository:

```csharp
/// Converts the domain events buffered on tracked aggregates into outbox rows right before SaveChanges,
/// so "state changed" and "event recorded" commit atomically — no dual write, no lost notification.
/// </summary>
public sealed class DomainEventsToOutboxInterceptor(OutboxEventRegistry registry)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        AddOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        AddOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddOutboxMessages(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var aggregates = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var messages = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(ToOutboxMessage)
            .ToList();

```

## The same change elsewhere in this repository

- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L8-L8]: `    public static ModelBuilder ApplyOutbox(this ModelBuilder modelBuilder)`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L14-L14]: `outbox.ToTable("outbox_messages");`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L35-L35]: `public sealed partial class OutboxProcessor<TDbContext>(`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L41-L41]: `where TDbContext : DbContext`
- `src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs` [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L11]: `public static IServiceCollection AddOutbox<TDbContext>(`

## Patterns

Required:
- `AddOutbox`
- `ApplyOutbox`
- `DomainEventsToOutboxInterceptor`
- `OutboxMessage`

## Workflow

1. Call modelBuilder.ApplyOutbox() inside the module DbContext OnModelCreating method to configure the outbox_messages table and indices. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L32-L32]
2. Attach DomainEventsToOutboxInterceptor to the DbContext options during registration so uncommitted domain events are converted to OutboxMessage entities before SaveChanges. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L12-L12]
3. Ensure OutboxOptions section is bound for PollingInterval and BatchSize configuration. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L22-L22]
4. Register any domain event types raised by aggregates so OutboxEventRegistry can whitelist them for serialization. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L12-L12]

## Checklist

- [ ] Call modelBuilder.ApplyOutbox() in the module DbContext OnModelCreating. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L32-L32]
- [ ] Add DomainEventsToOutboxInterceptor to the DbContext options. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L12-L12]
- [ ] Verify all passed event types implement IDomainEvent. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L53-L53]
- [ ] Verify OutboxOptions section exists in configuration if overriding default polling or batch size. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L22-L22]
- [ ] Pass all domain event types published by the module to AddOutbox. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L11]
- [ ] Configure the optional Outbox configuration section if custom PollingInterval or BatchSize is needed. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L26-L26]
- [ ] ApplyOutbox is called in OnModelCreating on the module DbContext [src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L8-L8]
- [ ] Verify that AddOutbox is called with the module DbContext type and configuration. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L11]
