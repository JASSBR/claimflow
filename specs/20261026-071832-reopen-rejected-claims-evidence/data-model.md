# Data Model: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | `/specs/20261026-071832-reopen-rejected-claims-evidence/`

## Domain Entities & Enums (`ClaimFlow.Claims.Domain`)

### 1. `ClaimAction` (`src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimEnums.cs`)

| Member | From Status(es) | To Status | Needs Reason | Needs Amount | Roles Permitted |
|---|---|---|---|---|---|
| `StartReview` | `Declared` | `UnderReview` | No | No | `Handler`, `Manager` |
| `RequestInformation` | `UnderReview` | `InformationRequested` | Yes | No | `Handler`, `Manager` |
| `ResumeReview` | `InformationRequested` | `UnderReview` | No | No | `Handler`, `Manager` |
| `Approve` | `UnderReview` | `Approved` | No | Yes | `Handler` (≤ limit), `Manager` |
| `Reject` | `UnderReview`, `InformationRequested` | `Rejected` | Yes | No | `Handler`, `Manager` |
| `Settle` | `Approved` | `Settled` | No | No | `Manager` (≠ approver) |
| **`Reopen`** *(new)* | **`Rejected`** | **`UnderReview`** | **Yes** | **No** | **`Handler`, `Manager`** |

### 2. `Claim` Aggregate Root (`src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs`)

- **Constants**:
  - `public const int ReopenWindowDays = 30;` (or `private static readonly TimeSpan ReopenWindow = TimeSpan.FromDays(30);`)
- **Workflow Table**:
  - Adds `[ClaimAction.Reopen] = ([ClaimStatus.Rejected], ClaimStatus.UnderReview)` to `Workflow`.
- **Domain Method `Reopen(Actor actor, string? reason, DateTimeOffset now)`**:
  1. Validates `actor` is non-null (`ArgumentNullException.ThrowIfNull(actor)`).
  2. If `Status != ClaimStatus.Rejected`, returns `ClaimErrors.TransitionNotAllowed(ClaimAction.Reopen, Status)` (`409 Conflict`).
  3. If `string.IsNullOrWhiteSpace(reason)`, returns `ClaimErrors.ReasonRequired` (`400 ValidationProblem`).
  4. If `_history.Any(change => change.Action == ClaimAction.Reopen)`, returns `ClaimErrors.AlreadyReopened` (`409 Conflict`).
  5. Finds the most recent rejection entry in `_history` (`_history.LastOrDefault(change => change.Action == ClaimAction.Reject)`). If `now - rejectedAt > TimeSpan.FromDays(ReopenWindowDays)`, returns `ClaimErrors.ReopenWindowExpired` (`409 Conflict`).
  6. Calls `Transition(ClaimAction.Reopen, actor, reason.Trim(), now)`, which sets `Status = ClaimStatus.UnderReview`, updates `LastUpdatedAt = now`, appends `ClaimStatusChange(ClaimStatus.Rejected, ClaimStatus.UnderReview, ClaimAction.Reopen, actor, reason.Trim(), now)` to `_history`, and raises `ClaimStatusChanged`.
- **Eligibility Evaluation `AllowedActionsFor(Actor actor, DateTimeOffset now)`**:
  - Filters `AllowedActions` to:
    - Exclude `ClaimAction.Settle` when `IsApprover(actor)` (existing four-eyes rule).
    - Exclude `ClaimAction.Reopen` when `!CanReopen(now)` (i.e., when `_history.Any(change => change.Action == ClaimAction.Reopen)` or `now - lastReject.OccurredAt > TimeSpan.FromDays(ReopenWindowDays)`).

### 3. `ClaimErrors` (`src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs`)

| Member | Code | Type | HTTP Status | Default Description |
|---|---|---|---|---|
| `ReasonRequired` *(existing)* | `claim.reason_required` | `Validation` | `400 Bad Request` | `"A reason is required for this decision."` |
| `ReopenWindowExpired` *(new)* | `claim.reopen_window_expired` | `Conflict` | `409 Conflict` | `"A rejected claim can only be reopened within 30 days of rejection."` |
| `AlreadyReopened` *(new)* | `claim.already_reopened` | `Conflict` | `409 Conflict` | `"A claim can only be reopened once."` |
| `TransitionNotAllowed` *(existing)* | `claim.transition_not_allowed` | `Conflict` | `409 Conflict` | `"Action '{action}' is not allowed while the claim is '{status}'."` |

### 4. Database Schema (`claims.claims` and `claims.claim_history`)

- **No schema changes or EF Core migrations**:
  - `claims.claim_history.action` is `character varying(32)` mapped via `.HasConversion<string>().HasMaxLength(32)`. `"Reopen"` (6 chars) persists without DDL changes.
  - `claims.claim_history.occurred_at` (`timestamp with time zone`) is already persisted on every `ClaimStatusChange` row and loaded eagerly with the `Claim` aggregate (`OwnsMany`).
