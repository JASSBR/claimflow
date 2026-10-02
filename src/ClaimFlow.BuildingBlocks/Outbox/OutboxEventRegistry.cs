using System.Collections.Frozen;
using ClaimFlow.SharedKernel;

namespace ClaimFlow.BuildingBlocks.Outbox;

/// <summary>
/// Closed list of event types the outbox may (de)serialize. Stored type names are resolved through this map,
/// never through Type.GetType: a tampered row can't make the processor instantiate an arbitrary type.
/// </summary>
/// <summary>The event types one module contributes to the outbox. Each module registers its own.</summary>
public sealed record OutboxEventTypes(IReadOnlyList<Type> Types);

public sealed class OutboxEventRegistry
{
    private readonly FrozenDictionary<string, Type> _typesByName;

    public OutboxEventRegistry(IEnumerable<OutboxEventTypes> contributions)
    {
        ArgumentNullException.ThrowIfNull(contributions);

        var types = contributions.SelectMany(contribution => contribution.Types).Distinct().ToList();
        var invalid = types.Find(type => !typeof(IDomainEvent).IsAssignableFrom(type));
        if (invalid is not null)
        {
            throw new ArgumentException($"{invalid.Name} does not implement {nameof(IDomainEvent)}.", nameof(contributions));
        }

        _typesByName = types.ToFrozenDictionary(NameOf, StringComparer.Ordinal);
    }

    public static string NameOf(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        return eventType.FullName ?? eventType.Name;
    }

    public bool TryResolve(string name, out Type eventType) => _typesByName.TryGetValue(name, out eventType!);

    public bool IsRegistered(Type eventType) => _typesByName.ContainsKey(NameOf(eventType));
}
