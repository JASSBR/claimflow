namespace ClaimFlow.BuildingBlocks.Outbox;

/// <summary>A domain event persisted in the same transaction as the state change that raised it.</summary>
public sealed class OutboxMessage
{
    public const int MaxAttempts = 5;

    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Type { get; init; }

    public required string Payload { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }
}
