# Research: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | `/specs/20261026-071832-reopen-rejected-claims-evidence/`

Each entry records a decision, the rationale behind it, and the alternatives considered.

## Decided by the estate

### Settled claims cannot be reopened

- **Decision**: Build to this criterion as written (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved). `ClaimStatus.Settled` remains terminal with no workflow entries originating from `Settled`.
- **Rationale**: Approved knowledge of the estate (`tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143`) for story "Reopen a rejected claim within 30 days" (`KED-28`).
- **Alternatives considered**: None; changing it would violate governed knowledge and business requirements.

## Resolved Design Decisions

### Language, version and dependencies

- **Decision**: Use the existing stack without adding packages: C# / .NET 10 (`ClaimFlow.slnx`, ASP.NET Core Minimal APIs, EF Core Npgsql, SignalR) and TypeScript / Angular 22 (`web/`, zoneless signals, `httpResource`, `@angular/localize` on Node.js 24 per `.nvmrc`).
- **Rationale**: Recorded in `CLAUDE.md`, `README.md`, and `.github/copilot-instructions.md`.
- **Alternatives considered**: None; this feature fits cleanly inside the existing `Claims` module and Angular SPA.

### Storage and persistence of reopen eligibility

- **Decision**: Derive both the 30-day rejection window and the single-reopen limit directly from the claim's existing `_history` (`claim_history` table) without adding columns or EF Core migrations.
- **Rationale**: `ClaimConfiguration.cs` already maps `ClaimStatusChange.Action`, `From`, and `To` as `string` (`HasConversion<string>().HasMaxLength(32)`) and records `OccurredAt` (`DateTimeOffset`). The most recent `ClaimAction.Reject` entry gives the exact rejection timestamp, and `History.Any(h => h.Action == ClaimAction.Reopen)` answers whether the claim was already reopened once.
- **Alternatives considered**: Adding `RejectedAt` and `ReopenedCount` columns to `claims.claims` — rejected as redundant state that could drift from `_history` and would require an unnecessary schema migration.

### Time-dependent `AllowedActionsFor` and ADR 0010

- **Decision**: Add `[ClaimAction.Reopen] = ([ClaimStatus.Rejected], ClaimStatus.UnderReview)` to `Claim.Workflow`. Keep `Claim.AllowedActions` as the pure static workflow lookup by status, and extend `Claim.AllowedActionsFor(Actor actor, DateTimeOffset now)` to filter out `ClaimAction.Reopen` when the claim has already been reopened once or when `now - rejectedAt > TimeSpan.FromDays(30)`. `ClaimPermissions.AllowedActions(Claim claim, ClaimsPrincipal user, DateTimeOffset now)` passes `TimeProvider.GetUtcNow()` from `GetClaim` and `ApplyClaimAction`. Document this architectural decision in `docs/adr/0010-time-dependent-allowed-actions.md`.
- **Rationale**: Preserves Principle I (server-driven workflow where the UI contains zero workflow or date math) and Principle IV (all current time comes from `TimeProvider`).
- **Alternatives considered**:
  - Computing the 30-day window in the Angular UI — rejected because it violates ADR 0007 and Principle I ("the UI renders only the actions the server allows").
  - Calling `DateTimeOffset.UtcNow` inside `Claim.AllowedActions` — rejected because it violates Principle IV (deterministic time via `TimeProvider`).

### Domain error codes for reopen refusals

- **Decision**: Reuse `ClaimErrors.ReasonRequired` (`claim.reason_required`, Validation / 400) when the reopen reason is null or whitespace; add `ClaimErrors.ReopenWindowExpired` (`claim.reopen_window_expired`, Conflict / 409) when more than 30 days have elapsed since the rejection; and add `ClaimErrors.AlreadyReopened` (`claim.already_reopened`, Conflict / 409) when the claim's history already contains a `Reopen` entry. Calling `Reopen` from any status other than `Rejected` returns `ClaimErrors.TransitionNotAllowed(ClaimAction.Reopen, Status)` (`claim.transition_not_allowed`, Conflict / 409).
- **Rationale**: Follows ADR 0005 (`Result` + RFC 9457 `ProblemDetails`) and skill `define-domain-errors`.
- **Alternatives considered**: Returning a generic `claim.transition_not_allowed` for expired window or second reopen — rejected because distinct error codes allow clear user-facing messages in FR and EN and explicit test assertions for each business criterion.

### Resolving acknowledged estate conflicts (`rul_b31a` and `rul_8b9d`)

- **Decision**: Intentionally override `ko_b31a` (`gradera:b31a7f0c-4be0-409b-9ce4-f54c141aa429`, approved) and `ko_8b9d` (`gradera:8b9d4e71-e5df-4eca-907d-68e211227613`, approved) for `Rejected` claims in the claims portal (`ClaimFlow`), allowing `Rejected -> UnderReview` via `ClaimAction.Reopen` within 30 days at most once.
- **Rationale**: Explicitly acknowledged in `KED-28` (`CH-1`, `AC-1.1`, `AC-1.3`) and authorized by Principle III of `.specify/memory/constitution.md`.
- **Alternatives considered**: Creating a separate `Reopened` status — rejected because `KED-28` specifies transitioning back into `UnderReview` and following the normal review path.

### UI metadata, localization, and demo seed data

- **Decision**:
  - Add `'Reopen'` to `ClaimAction` in `web/src/app/claims/claim.models.ts`.
  - Add `Reopen` to `ACTION_META` in `web/src/app/claims/claim-labels.ts` with `label: $localize`:@@action.reopen:Rouvrir le dossier``, `hint: $localize`:@@action.reopen.hint:Nouvelles pièces reçues dans les 30 jours suivant le refus.``, `needsReason: true`, `needsAmount: false`, `tone: 'primary'`.
  - Map `claim.reopen_window_expired` and `claim.already_reopened` in `web/src/app/shared/problem-details.ts` and add English translations (`"Reopen"`, `"New evidence received within 30 days of rejection."`, and error texts) in `web/tools/translations.en.json`, then run `npm --prefix web run i18n`.
  - Add a sample claim in `ClaimsSeeder.cs` rejected ~10 days ago so a fresh environment immediately showcases a reopenable rejected claim alongside the existing 90-day-old rejected claim (`POL-509114`, where the 30-day window has expired).
- **Rationale**: Satisfies Principle V (bilingual UI), Principle I (server-driven decision panel), and makes both the within-window and expired-window states immediately demonstrable on startup.
- **Alternatives considered**: Modifying the existing 90-day rejected sample in `ClaimsSeeder.cs` — rejected because keeping both a 90-day rejected claim (closed, window expired) and a ~10-day rejected claim (reopenable) demonstrates both scenarios out of the box.
