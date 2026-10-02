namespace ClaimFlow.Documents.Analysis;

/// <summary>
/// A stored AI review of a claim file. Kept (not regenerated on every view) because it costs money, because the
/// handler must see exactly what the AI said when they decided, and because "who asked, which model, which files"
/// belongs in the audit trail.
/// </summary>
internal sealed class ClaimAnalysis
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public Guid ClaimId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public required string RequestedById { get; init; }

    public required string RequestedByName { get; init; }

    public required string Model { get; init; }

    public bool Refused { get; init; }

    public required string Content { get; init; }

    public List<AnalysisCitation> Citations { get; init; } = [];

    public int DocumentCount { get; init; }

    public long InputTokens { get; init; }

    public long OutputTokens { get; init; }
}
