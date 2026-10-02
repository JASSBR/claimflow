using ClaimFlow.Claims.Domain;

namespace ClaimFlow.Claims.Contracts;

public sealed record DeclareClaimRequest(
    string PolicyNumber,
    ClaimType Type,
    DateOnly IncidentDate,
    string Description,
    decimal ClaimedAmount);

/// <param name="ExpectedVersion">The version the user saw. A mismatch means someone else changed the claim meanwhile → 409.</param>
public sealed record ClaimActionRequest(ClaimAction Action, uint ExpectedVersion, string? Reason, decimal? ApprovedAmount);
