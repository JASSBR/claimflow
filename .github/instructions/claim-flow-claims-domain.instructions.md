---
applyTo: 'src/Modules/Claims/ClaimFlow.Claims.Domain/**'
---

# ClaimFlow.Claims.Domain

`src/Modules/Claims/ClaimFlow.Claims.Domain/` is a dotnet project of this repository, declared by `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj`

## Depends on

Depends on `claim-flow-shared-kernel` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] A claim approval cannot exceed the user's approval authority limit in EUR; amounts exceeding the authority limit require approval by a claims manager. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L41-L43]
- [Invariant] Every status transition must conform to the static Workflow mapping dictionary; on transition, Status is updated, LastUpdatedAt is set to now, a ClaimStatusChange record is appended to _history, and a ClaimStatusChanged domain event is raised. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L199-L210]
- [Invariant] Amounts in the claim domain are always rounded to 2 decimal places using MidpointRounding.ToEven (banker's rounding). [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L195-L195]
- [BusinessRule] The claimed amount must be strictly greater than 0 EUR and cannot exceed Claim.MaxClaimedAmount EUR. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L22-L23]
- [BusinessRule] Approving a claim transitions it from UnderReview to Approved, requiring the rounded approved amount to be greater than zero, no greater than the ClaimedAmount, and within the approving actor's authority limit. Upon success, ApprovedAmount and ApprovedById are recorded. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L125-L148]
- [BusinessRule] The plan's limit applies: 12 months from the date of service for out-of-network claims. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L28-L29]
- [BusinessRule] A claim in the claims portal is in one of six statuses: Declared, UnderReview, InformationRequested, Approved, Rejected, Settled. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimEnums.cs:L10-L18]
- [BusinessRule] AllowedActionsFor filters the claim's currently valid workflow actions to exclude ClaimAction.Settle if the specified actor was the one who approved the claim. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L82-L87]
- [BusinessRule] A claim action cannot be performed if it is not a valid transition from the claim's current status. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L43-L44]
- [BusinessRule] ClaimStatusChange audit records are append-only and immutable; once created and written, they must never be updated. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L5-L5]
- [BusinessRule] Under the four-eyes principle, the user who approved a claim is prohibited from releasing its payment. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L37-L38]
- [BusinessRule] Declaring a new claim requires valid claim data, a non-whitespace business reference number, and an actor; on success, the claim is initialized in Declared status with its claimed amount rounded to two decimal places, and emits a ClaimDeclared domain event. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L90-L114]
- [BusinessRule] A claim cannot be declared for an incident date in the future. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L13-L14]
- [BusinessRule] An approver cannot approve a claim amount that exceeds their assigned approval authority limit; approval requires a claims manager. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L40-L41]
- [BusinessRule] Starting a review transitions the claim from Declared status to UnderReview status without requiring a reason. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L116-L116]
- [BusinessRule] Requesting information transitions a claim from UnderReview to InformationRequested, and requires a non-null, non-whitespace reason. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L118-L121]
- [BusinessRule] Four-eyes principle: the person who approved a claim's amount cannot also settle (pay) it; Settle on an Approved claim by its approver fails with FourEyesViolation. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L164-L171]
- [BusinessRule] A policy number associated with a claim must conform to the format 'POL-000000'. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L7-L8]
- [BusinessRule] Created by npm run smoke -- --writes. Safe to reject. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L164-L171]
- [BusinessRule] A decision reason must be provided when making a claim decision. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L25-L26]
- [BusinessRule] The description of a claim must be between Claim.DescriptionMinLength and Claim.DescriptionMaxLength characters in length. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L19-L20]
- [BusinessRule] Claims are time-barred two years after the incident occurs pursuant to Code des assurances L114-1. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L16-L17]
- [BusinessRule] The claims portal refuses a claim declaration whose incident date is more than 2 years before today (LimitationPeriodYears = 2, L114-1 Code des assurances), with the error IncidentTimeBarred. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L28-L29]
- [InformalDecision] The Claims.Domain module relies strictly on ClaimFlow.SharedKernel without referencing EF Core or ASP.NET Core, keeping domain logic isolated from persistence and web frameworks. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L1-L3]

## Skills

- `add-aggregate-root`: Define a domain aggregate root inheriting from AggregateRoot<TId> that buffers domain events for outbox persistence. (`.github/skills/add-aggregate-root/SKILL.md`)
- `add-domain-entity`: Implement domain entities and aggregate roots in Domain projects referencing only SharedKernel with private materialization constructors. (`.github/skills/add-domain-entity/SKILL.md`)
- `add-workflow-transition`: Add or extend a state machine transition on the Claim aggregate root with audit history and authority checks. (`.github/skills/add-workflow-transition/SKILL.md`)
