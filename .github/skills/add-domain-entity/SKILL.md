---
name: add-domain-entity
description: "Implement domain entities and aggregate roots in Domain projects referencing only SharedKernel with private materialization constructors."
---

> Evidence: value gate verified the facts below against this repository.

Implement domain entities and aggregate roots in Domain projects referencing only SharedKernel with private materialization constructors.

Applies in: `claim-flow-claims-domain`.

## What is specific to this repository

- The claims domain model references only SharedKernel, without EF Core or ASP.NET Core dependencies. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj:L2-L2]
- Claim aggregate root inherits from AggregateRoot parameterized with ClaimId. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L22-L22]
- Entities use private parameterless constructors documented for persistence layer materialization. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L44-L47]
- ClaimStatusChange generates new entity identifiers using Guid.CreateVersion7(). [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L10-L10]

## Rules this change touches

- [Invariant] Every status transition must conform to the static Workflow mapping dictionary; on transition, Status is updated, LastUpdatedAt is set to now, a ClaimStatusChange record is appended to _history, and a ClaimStatusChanged domain event is raised. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L199-L210)
- [Invariant] Amounts in the claim domain are always rounded to 2 decimal places using MidpointRounding.ToEven (banker's rounding). (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L195-L195)
- [BusinessRule] Approving a claim transitions it from UnderReview to Approved, requiring the rounded approved amount to be greater than zero, no greater than the ClaimedAmount, and within the approving actor's authority limit. Upon success, ApprovedAmount and ApprovedById are recorded. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L125-L148)
- [BusinessRule] The plan's limit applies: 12 months from the date of service for out-of-network claims. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L28-L29)
- [BusinessRule] AllowedActionsFor filters the claim's currently valid workflow actions to exclude ClaimAction.Settle if the specified actor was the one who approved the claim. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L82-L87)
- [BusinessRule] ClaimStatusChange audit records are append-only and immutable; once created and written, they must never be updated. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L5-L5)
- [BusinessRule] Declaring a new claim requires valid claim data, a non-whitespace business reference number, and an actor; on success, the claim is initialized in Declared status with its claimed amount rounded to two decimal places, and emits a ClaimDeclared domain event. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L90-L114)
- [BusinessRule] Starting a review transitions the claim from Declared status to UnderReview status without requiring a reason. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L116-L116)
- [BusinessRule] Requesting information transitions a claim from UnderReview to InformationRequested, and requires a non-null, non-whitespace reason. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L118-L121)
- [BusinessRule] Four-eyes principle: the person who approved a claim's amount cannot also settle (pay) it; Settle on an Approved claim by its approver fails with FourEyesViolation. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L164-L171)

## Context

- ClaimStatusChange provides a private parameterless constructor specifically for ORM / persistence layer materialization. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L19-L22]
- Claim defines an optimistic concurrency token property Version of type uint mapped to PostgreSQL's xmin system column, where only the database writes the column value and EF Core populates the compiler-generated backing field. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L66-L68]
- ClaimStatusChange parameterized constructor initializes a status change record by generating a Version 7 UUID (Guid.CreateVersion7()) for Id, copying the from and to statuses, action, actor ID and name, optional reason, and occurredAt timestamp. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L8-L17]

## Start here

`src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj` [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj:L1-L6] is the worked example for this change, as it stands in the repository:

```
<Project Sdk="Microsoft.NET.Sdk">
  <!-- The claims domain model: depends on SharedKernel only. No EF Core, no ASP.NET Core (enforced by ArchitectureTests). -->
  <ItemGroup>
    <ProjectReference Include="..\..\..\ClaimFlow.SharedKernel\ClaimFlow.SharedKernel.csproj" />
  </ItemGroup>
</Project>
```

## The same change elsewhere in this repository

- `src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L22-L22]: `public sealed partial class Claim : AggregateRoot<ClaimId>`
- `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L6-L6]: `public sealed class ClaimStatusChange`
- `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L10-L10]: `Id = Guid.CreateVersion7();`
- `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L20-L20]: `private ClaimStatusChange()`
- `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L27-L27]: `public ClaimStatus From { get; private set; }`

## Patterns

Required:
- `AggregateRoot`
- `Guid.CreateVersion7()`
- `private set`
Forbidden:
- Avoid `Microsoft.AspNetCore` in this change.
- Avoid `Microsoft.EntityFrameworkCore` in this change.

## Workflow

1. Ensure the Domain csproj references only ClaimFlow.SharedKernel without EF Core or ASP.NET Core references. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj:L4-L4]
2. Create the entity class, inheriting from AggregateRoot<TId> if it is an aggregate root. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L22-L22]
3. Initialize identifiers with Guid.CreateVersion7() for new instances. [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L10-L10]

## Checklist

- [ ] Domain project references only ClaimFlow.SharedKernel and avoids EF Core or ASP.NET Core [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj:L4-L4]
- [ ] Aggregate root inherits from AggregateRoot<TId> [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L22-L22]
- [ ] Entity identifiers are generated using Guid.CreateVersion7() [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs:L10-L10]
