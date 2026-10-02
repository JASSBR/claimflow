namespace ClaimFlow.Claims.Domain;

/// <summary>Strongly typed id: a ClaimId can never be passed where a PolicyId is expected.</summary>
public readonly record struct ClaimId(Guid Value)
{
    // Version 7 GUIDs are time-ordered, so inserts stay append-only in the primary key index.
    public static ClaimId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}
