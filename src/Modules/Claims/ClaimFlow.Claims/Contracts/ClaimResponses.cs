using ClaimFlow.Claims.Domain;

namespace ClaimFlow.Claims.Contracts;

public sealed record ClaimSummaryResponse(
    Guid Id,
    string Number,
    string PolicyNumber,
    ClaimType Type,
    ClaimStatus Status,
    decimal ClaimedAmount,
    decimal? ApprovedAmount,
    DateOnly IncidentDate,
    DateTimeOffset DeclaredAt,
    DateTimeOffset LastUpdatedAt);

public sealed record ClaimHistoryResponse(
    ClaimStatus From,
    ClaimStatus To,
    ClaimAction Action,
    string ActorName,
    string? Reason,
    DateTimeOffset OccurredAt);

public sealed record ClaimDetailsResponse(
    Guid Id,
    string Number,
    string PolicyNumber,
    ClaimType Type,
    ClaimStatus Status,
    string Description,
    decimal ClaimedAmount,
    decimal? ApprovedAmount,
    DateOnly IncidentDate,
    DateTimeOffset DeclaredAt,
    string DeclaredBy,
    DateTimeOffset LastUpdatedAt,
    uint Version,
    IReadOnlyList<ClaimAction> AllowedActions,
    IReadOnlyList<ClaimHistoryResponse> History)
{
    /// <param name="allowedActions">Already filtered for the current user (workflow, role, segregation of duties).</param>
    internal static ClaimDetailsResponse From(Claim claim, IReadOnlyList<ClaimAction> allowedActions) => new(
        claim.Id.Value,
        claim.Number,
        claim.PolicyNumber,
        claim.Type,
        claim.Status,
        claim.Description,
        claim.ClaimedAmount,
        claim.ApprovedAmount,
        claim.IncidentDate,
        claim.DeclaredAt,
        claim.DeclaredByName,
        claim.LastUpdatedAt,
        claim.Version,
        allowedActions,
        [.. claim.History
            .OrderBy(change => change.OccurredAt)
            .ThenBy(change => change.Id)
            .Select(change => new ClaimHistoryResponse(change.From, change.To, change.Action, change.ActorName, change.Reason, change.OccurredAt))]);
}

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public sealed record ClaimStatsResponse(
    IReadOnlyDictionary<ClaimStatus, int> CountByStatus,
    decimal TotalClaimedAmount,
    decimal TotalApprovedAmount);

/// <summary>Pushed to SignalR clients after a claim change is committed.</summary>
public sealed record ClaimChangedNotification(Guid ClaimId, string Number, ClaimStatus Status, string ActorName, DateTimeOffset OccurredAt);

/// <summary>What the current user may do at portfolio level, so the UI can adapt before any claim is opened.</summary>
public sealed record ClaimCapabilitiesResponse(bool CanDeclare, decimal ApprovalLimit, IReadOnlyList<string> Roles);
