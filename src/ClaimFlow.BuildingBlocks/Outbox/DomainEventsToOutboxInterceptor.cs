using System.Text.Json;
using ClaimFlow.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClaimFlow.BuildingBlocks.Outbox;

/// <summary>
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

        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());
        context.Set<OutboxMessage>().AddRange(messages);
    }

    private OutboxMessage ToOutboxMessage(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();
        if (!registry.IsRegistered(type))
        {
            throw new InvalidOperationException($"Domain event {type.Name} is not registered in the outbox registry.");
        }

        return new OutboxMessage
        {
            Type = OutboxEventRegistry.NameOf(type),
            Payload = JsonSerializer.Serialize(domainEvent, type, JsonSerializerOptions.Web),
            OccurredAt = domainEvent.OccurredAt,
        };
    }
}
