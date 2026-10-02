using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain;

public sealed record ClaimDeclared(ClaimId ClaimId, string Number, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record ClaimStatusChanged(
    ClaimId ClaimId,
    string Number,
    ClaimStatus From,
    ClaimStatus To,
    DateTimeOffset OccurredAt) : IDomainEvent;
