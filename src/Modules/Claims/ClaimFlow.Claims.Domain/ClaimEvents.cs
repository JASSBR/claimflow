using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain;

public sealed record ClaimDeclared(ClaimId ClaimId, string Number, string ActorName, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record ClaimStatusChanged(
    ClaimId ClaimId,
    string Number,
    ClaimStatus From,
    ClaimStatus To,
    string ActorName,
    DateTimeOffset OccurredAt) : IDomainEvent;
