---
name: add-workflow-transition
description: "Add or extend a state machine transition on the Claim aggregate root with audit history and authority checks."
---

> Evidence: value gate verified the facts below against this repository.

Add or extend a state machine transition on the Claim aggregate root with audit history and authority checks.

Applies in: `claim-flow-claims-domain`.

## What is specific to this repository

- Claim workflow transitions are defined in a static FrozenDictionary mapping ClaimAction to allowed source ClaimStatus values and a target ClaimStatus. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L31-L40]
- Approval power is modeled via an ApprovalAuthority record pairing an Actor with a decimal Limit. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L15-L15]
- The claim tracks ApprovedById to enforce a four-eyes segregation of duties rule at settlement when computing allowed actions for an actor. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L85-L90]

## Rules this change touches

- [Invariant] Every status transition must conform to the static Workflow mapping dictionary; on transition, Status is updated, LastUpdatedAt is set to now, a ClaimStatusChange record is appended to _history, and a ClaimStatusChanged domain event is raised. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L199-L210)
- [Invariant] Amounts in the claim domain are always rounded to 2 decimal places using MidpointRounding.ToEven (banker's rounding). (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L195-L195)
- [BusinessRule] Approving a claim transitions it from UnderReview to Approved, requiring the rounded approved amount to be greater than zero, no greater than the ClaimedAmount, and within the approving actor's authority limit. Upon success, ApprovedAmount and ApprovedById are recorded. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L125-L148)
- [BusinessRule] The plan's limit applies: 12 months from the date of service for out-of-network claims. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L28-L29)
- [BusinessRule] AllowedActionsFor filters the claim's currently valid workflow actions to exclude ClaimAction.Settle if the specified actor was the one who approved the claim. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L82-L87)
- [BusinessRule] Declaring a new claim requires valid claim data, a non-whitespace business reference number, and an actor; on success, the claim is initialized in Declared status with its claimed amount rounded to two decimal places, and emits a ClaimDeclared domain event. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L90-L114)
- [BusinessRule] Starting a review transitions the claim from Declared status to UnderReview status without requiring a reason. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L116-L116)
- [BusinessRule] Requesting information transitions a claim from UnderReview to InformationRequested, and requires a non-null, non-whitespace reason. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L118-L121)
- [BusinessRule] Four-eyes principle: the person who approved a claim's amount cannot also settle (pay) it; Settle on an Approved claim by its approver fails with FourEyesViolation. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L164-L171)
- [BusinessRule] Created by npm run smoke -- --writes. Safe to reject. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L164-L171)

## Context

- Claim defines an optimistic concurrency token property Version of type uint mapped to PostgreSQL's xmin system column, where only the database writes the column value and EF Core populates the compiler-generated backing field. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L66-L68]

## Start here

`src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L28-L67] is the worked example for this change, as it stands in the repository:

```csharp
    // L114-1 Code des assurances: actions arising from an insurance contract are time-barred after two years.
    private const int LimitationPeriodYears = 2;

    private static readonly FrozenDictionary<ClaimAction, (ClaimStatus[] From, ClaimStatus To)> Workflow =
        new Dictionary<ClaimAction, (ClaimStatus[] From, ClaimStatus To)>
        {
            [ClaimAction.StartReview] = ([ClaimStatus.Declared], ClaimStatus.UnderReview),
            [ClaimAction.RequestInformation] = ([ClaimStatus.UnderReview], ClaimStatus.InformationRequested),
            [ClaimAction.ResumeReview] = ([ClaimStatus.InformationRequested], ClaimStatus.UnderReview),
            [ClaimAction.Approve] = ([ClaimStatus.UnderReview], ClaimStatus.Approved),
            [ClaimAction.Reject] = ([ClaimStatus.UnderReview, ClaimStatus.InformationRequested], ClaimStatus.Rejected),
            [ClaimAction.Settle] = ([ClaimStatus.Approved], ClaimStatus.Settled),
        }.ToFrozenDictionary();

    private readonly List<ClaimStatusChange> _history = [];

    private Claim()
    {
        // Materialization constructor for the persistence layer.
    }

    public string Number { get; private set; } = string.Empty;

    public string PolicyNumber { get; private set; } = string.Empty;

    public ClaimType Type { get; private set; }

    public DateOnly IncidentDate { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal ClaimedAmount { get; private set; }

    public decimal? ApprovedAmount { get; private set; }

    public ClaimStatus Status { get; private set; }

    public DateTimeOffset DeclaredAt { get; private set; }

    public string DeclaredById { get; private set; } = string.Empty;
```

## Workflow

1. Add or identify the ClaimAction and ClaimStatus enum values for the transition. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L31-L31]
2. Register the allowed transition in the static Workflow FrozenDictionary mapping ClaimAction to allowed source statuses and target status. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L31-L31]
3. Implement the aggregate method on Claim to validate preconditions such as ApprovalAuthority or four-eyes rules. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L15-L15]

## Checklist

- [ ] Does Workflow map the ClaimAction to all valid source ClaimStatus values? [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L31-L31]
- [ ] Is every domain failure returning an Error from ClaimErrors rather than throwing? [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L128-L128]
- [ ] Does the transition verify four-eyes constraints (e.g. FourEyesViolation) when settling? [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L169-L169]
- [ ] Does the transition check ApprovalAuthority limits if approving an amount? [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L15-L15]
- [ ] Is an immutable ClaimStatusChange appended to _history for auditing? [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L42-L42]
