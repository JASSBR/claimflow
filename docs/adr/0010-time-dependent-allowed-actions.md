# 0010 — Time-dependent allowed actions and history-derived reopen eligibility

- **Status:** Accepted · 2026-10-26

## Context

Rejected claims can be reopened into `UnderReview` when new evidence arrives, subject to two domain invariants:

1. **30-day window**: Reopening is only permitted within 30 days of the rejection timestamp (`now - rejectedAt <= 30 days`). From day 31 onward, the claim stays closed.
2. **Single reopen limit**: A claim may be reopened at most once in its lifecycle; if it is rejected a second time, it cannot be reopened again.

By [ADR 0007](0007-frontend-signals-server-driven-workflow.md) and [ADR 0008](0008-authentication-and-segregation-of-duties.md), the server computes `allowedActions` and the Angular UI renders only the buttons in that list without client-side workflow logic. Previously, `Claim.AllowedActionsFor(Actor)` depended only on the static `Workflow` dictionary and `ApprovedById` (for the four-eyes rule), neither of which required the current time.

## Decision

- **`Claim.AllowedActionsFor(Actor actor, DateTimeOffset now)` takes the current timestamp** from `TimeProvider` (passed through `ClaimPermissions.AllowedActions(claim, user, now)` in `GetClaim`, `DeclareClaim`, and `ApplyClaimAction`).
  - `Claim.AllowedActions` remains the pure status-based lookup over the static `Workflow` table (`Rejected -> [Reopen]`).
  - `Claim.AllowedActionsFor(actor, now)` filters `Reopen` out when the claim has already been reopened once or when `now - lastRejection.OccurredAt > TimeSpan.FromDays(30)`.
- **Both reopen facts are read directly from `_history` (`claims.claim_history`)**:
  - Whether the claim was already reopened is `_history.Exists(change => change.Action == ClaimAction.Reopen)`.
  - When the claim was rejected is `_history.FindLast(change => change.Action == ClaimAction.Reject)?.OccurredAt`.
  - Because `ClaimStatusChange.Action` is stored as a `varchar(32)` string (`HasConversion<string>()`), no denormalized columns on `claims.claims` and no EF Core schema migration are needed.
- **Direct command enforcement**: If a client submits `Reopen` after the 30-day window or after a prior reopen, `Claim.Reopen(actor, reason, now)` returns `ClaimErrors.ReopenWindowExpired` (`409 claim.reopen_window_expired`) or `ClaimErrors.AlreadyReopened` (`409 claim.already_reopened`).

## Consequences

- ✅ The UI remains free of date arithmetic and workflow rules: after day 30 or after one reopen, the server omits `Reopen` from `allowedActions` and the decision panel shows `"Dossier clos."` / `"Claim closed."`.
- ✅ Time-dependent eligibility is deterministic and unit-tested at exact boundaries (day 30 vs day 30 + 1 tick / day 31) without touching system clocks.
- ✅ Zero schema drift: `_history` is the single source of truth for both the audit trail and reopen eligibility.
