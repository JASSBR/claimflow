using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain;

/// <summary>One line of the claim's audit trail. Append-only: never updated once written.</summary>
public sealed class ClaimStatusChange
{
    internal ClaimStatusChange(ClaimStatus from, ClaimStatus to, ClaimAction action, Actor actor, string? reason, DateTimeOffset occurredAt)
    {
        Id = Guid.CreateVersion7();
        From = from;
        To = to;
        Action = action;
        ActorId = actor.Id;
        ActorName = actor.Name;
        Reason = reason;
        OccurredAt = occurredAt;
    }

    private ClaimStatusChange()
    {
        // Materialization constructor for the persistence layer.
    }

    public Guid Id { get; private set; }

    public ClaimStatus From { get; private set; }

    public ClaimStatus To { get; private set; }

    public ClaimAction Action { get; private set; }

    public string ActorId { get; private set; } = string.Empty;

    public string ActorName { get; private set; } = string.Empty;

    public string? Reason { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
