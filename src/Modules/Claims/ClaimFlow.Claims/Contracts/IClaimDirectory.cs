namespace ClaimFlow.Claims.Contracts;

/// <summary>
/// What other modules may know about a claim. Deliberately flat (strings, no domain types):
/// a consumer depends on this contract, never on the Claims domain model.
/// </summary>
public sealed record ClaimSnapshot(
    Guid Id,
    string Number,
    string PolicyNumber,
    string Type,
    string Status,
    DateOnly IncidentDate,
    DateTimeOffset DeclaredAt,
    string Description,
    decimal ClaimedAmount,
    decimal? ApprovedAmount);

/// <summary>In-process query API of the Claims module for other modules.</summary>
public interface IClaimDirectory
{
    Task<ClaimSnapshot?> FindAsync(Guid claimId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ClaimSnapshot>> FindByPolicyAsync(string policyNumber, CancellationToken cancellationToken);
}
