# Implementation Plan: Reopen Rejected Claims with New Evidence

**Branch**: `20261026-071832-reopen-rejected-claims-evidence` | **Date**: 2026-10-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/20261026-071832-reopen-rejected-claims-evidence/spec.md`

## Summary

Allow claims handlers and managers to reopen a `Rejected` claim into `UnderReview` status once within 30 days of its rejection when a reason describing new evidence is provided. Add `ClaimAction.Reopen` (`[ClaimStatus.Rejected] -> ClaimStatus.UnderReview`) in `Claim.cs`; enforce mandatory reason (`ClaimErrors.ReasonRequired`), the 30-day window (`ClaimErrors.ReopenWindowExpired`, `claim.reopen_window_expired`), and the single-reopen rule (`ClaimErrors.AlreadyReopened`, `claim.already_reopened`) from `_history`; pass `now` from `TimeProvider` into `Claim.AllowedActionsFor(actor, now)` and `ClaimPermissions.AllowedActions(claim, user, now)` (documented in ADR 0010); expose `'Reopen'` in the Angular decision panel (`"Rouvrir le dossier"` in FR, `"Reopen"` in EN); and seed a sample claim rejected ~10 days ago so a fresh environment displays the button immediately.

## Technical Context

**Language/Version**: C# / .NET 10 (`ClaimFlow.slnx`), TypeScript / Angular 22 (`web/`, Node.js 24 per `.nvmrc`)

**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core (Npgsql), SignalR, Angular 22 (zoneless, Signals, `httpResource`, `@angular/localize`)

**Storage**: PostgreSQL (`claims` schema via `ClaimsDbContext`; `ClaimStatusChange` rows in `claims.claim_history` store `Action`, `From`, and `To` as `varchar(32)` strings via `HasConversion<string>()` — no database migration required)

**Testing**: xUnit v3 + Shouldly (`ClaimFlow.Claims.Domain.UnitTests`), NetArchTest (`ClaimFlow.ArchitectureTests`), `WebApplicationFactory` + Testcontainers PostgreSQL 18 (`ClaimFlow.IntegrationTests`), Vitest (`web/` unit tests)

**Project Type**: Modular monolith backend (`ClaimFlow.Api` + `Claims` and `Documents` modules) + Angular 22 SPA (`web/`) orchestrated by .NET Aspire

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Single-Source Workflow & Server-Driven UI**: PASS — `ClaimAction.Reopen` (`[ClaimStatus.Rejected] -> ClaimStatus.UnderReview`) is added to `Claim.Workflow` in `Claim.cs`. Because eligibility depends on the 30-day window and single-reopen limit, `Claim.AllowedActionsFor(actor, now)` evaluates both rules on the server and `ClaimPermissions.AllowedActions(claim, user, now)` passes `TimeProvider.GetUtcNow()`. The Angular UI renders `claim().allowedActions` directly with no client-side workflow or date logic.
- **II. Test-First Discipline (NON-NEGOTIABLE)**: PASS — Domain unit tests (`ClaimWorkflowTests.cs`) are written first, followed by Testcontainers PostgreSQL integration tests (`ClaimsApiTests.cs`), then the domain/API implementation, and Vitest specs (`decision-panel.spec.ts` / `claim-detail.spec.ts`) for the UI changes.
- **III. Estate Knowledge Governance**: PASS — The intentional override of `ko_b31a` (`gradera:b31a7f0c-4be0-409b-9ce4-f54c141aa429`, approved) and `ko_8b9d` (`gradera:8b9d4e71-e5df-4eca-907d-68e211227613`, approved) is documented in `spec.md` and `research.md`, while preserving `Settled` finality (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved), four-eyes (`gradera:c7c74113-1b8b-46f8-bc31-3df9ea3fa38e`, approved), handler approval limits (`gradera:3aff2147-43bf-44d2-b1fd-f0c78b8a1041`, approved), and the 2-year time bar (`gradera:f479f3df-d99f-4370-bd16-04f77f4cb3d2`, approved).
- **IV. Explicit Error Handling & Deterministic Time**: PASS — Reopen failures return `Result` values (`ClaimErrors.ReasonRequired`, `ClaimErrors.ReopenWindowExpired`, `ClaimErrors.AlreadyReopened`, `ClaimErrors.TransitionNotAllowed`) mapped via `ToProblem()`. All current time comes from `TimeProvider` injected into `GetClaim`, `ApplyClaimAction`, and `ClaimsSeeder`.
- **V. Bilingual UI Localization (FR / EN)**: PASS — `ACTION_META.Reopen` (`"Rouvrir le dossier"` / `"Nouvelles pièces reçues dans les 30 jours suivant le refus."` in FR; `"Reopen"` / `"New evidence received within 30 days of rejection."` in EN) and error messages in `shared/problem-details.ts` are extracted and compiled via `web/tools/translations.en.json` and `npm --prefix web run i18n`.
- **VI. Audited State Changes & Transactional Realtime Outbox**: PASS — `Claim.Reopen` calls `Transition(ClaimAction.Reopen, actor, reason.Trim(), now)`, appending a `ClaimStatusChange` audit entry and raising `ClaimStatusChanged` through the transactional outbox to SignalR, guarded by `xmin` (`expectedVersion`).
- **VII. Zero-Warning Quality Gate**: PASS — Verified with `dotnet build ClaimFlow.slnx`, `dotnet test --solution ClaimFlow.slnx`, `dotnet format ClaimFlow.slnx --verify-no-changes`, and `npm --prefix web run {i18n,format:check,lint,test,build}`.

*Post-Phase 1 Design Re-check*: PASS — All design artifacts (`research.md`, `data-model.md`, `contracts/reopen-claim.md`, `quickstart.md`) conform to Principles I–VII with zero violations.

## Project Structure

### Documentation (this feature)

```text
specs/20261026-071832-reopen-rejected-claims-evidence/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── reopen-claim.md
├── tasks.md
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
docs/
└── adr/
    ├── README.md
    └── 0010-time-dependent-allowed-actions.md
src/
└── Modules/
    └── Claims/
        ├── ClaimFlow.Claims.Domain/
        │   ├── Claim.cs
        │   ├── ClaimEnums.cs
        │   └── ClaimErrors.cs
        └── ClaimFlow.Claims/
            ├── Features/
            │   ├── ApplyClaimAction.cs
            │   └── GetClaim.cs
            ├── Persistence/
            │   └── ClaimsSeeder.cs
            └── Security/
                └── ClaimPermissions.cs
tests/
├── ClaimFlow.Claims.Domain.UnitTests/
│   ├── ClaimBuilder.cs
│   └── ClaimWorkflowTests.cs
└── ClaimFlow.IntegrationTests/
    └── ClaimsApiTests.cs
web/
├── src/
│   ├── app/
│   │   ├── claims/
│   │   │   ├── claim-labels.ts
│   │   │   └── claim.models.ts
│   │   ├── pages/
│   │   │   └── claim-detail/
│   │   │       └── decision-panel.spec.ts
│   │   └── shared/
│   │       ├── problem-details.spec.ts
│   │       └── problem-details.ts
│   └── locale/
│       └── messages.en.xlf
└── tools/
    └── translations.en.json
```

**Structure Decision**: Modify the existing `Claims` vertical module (`ClaimFlow.Claims.Domain` and `ClaimFlow.Claims`), backend unit/integration test suites (`ClaimFlow.Claims.Domain.UnitTests`, `ClaimFlow.IntegrationTests`), Angular SPA (`web/`), and ADR catalog (`docs/adr/0010-time-dependent-allowed-actions.md`).

## Design Details

1. **Domain (`ClaimFlow.Claims.Domain`)**:
   - `ClaimEnums.cs`: Add `Reopen` to `ClaimAction`.
   - `ClaimErrors.cs`: Add `ReopenWindowExpired` (`Error.Conflict("claim.reopen_window_expired", "A rejected claim can only be reopened within 30 days of rejection.")`) and `AlreadyReopened` (`Error.Conflict("claim.already_reopened", "A claim can only be reopened once.")`).
   - `Claim.cs`:
     - Add `public const int ReopenWindowDays = 30;` and `[ClaimAction.Reopen] = ([ClaimStatus.Rejected], ClaimStatus.UnderReview)` to `Workflow`.
     - Add `public Result Reopen(Actor actor, string? reason, DateTimeOffset now)`:
       - Validates `actor` is non-null.
       - If `Status != ClaimStatus.Rejected`, returns `ClaimErrors.TransitionNotAllowed(ClaimAction.Reopen, Status)`.
       - If `string.IsNullOrWhiteSpace(reason)`, returns `ClaimErrors.ReasonRequired`.
       - If `_history.Any(change => change.Action == ClaimAction.Reopen)`, returns `ClaimErrors.AlreadyReopened`.
       - Checks the latest `ClaimAction.Reject` entry in `_history`; if `now - rejectedAt > TimeSpan.FromDays(ReopenWindowDays)`, returns `ClaimErrors.ReopenWindowExpired`.
       - Calls `Transition(ClaimAction.Reopen, actor, reason.Trim(), now)`.
     - Update `AllowedActionsFor(Actor actor, DateTimeOffset now)` to filter out `ClaimAction.Reopen` when `!CanReopen(now)` (where `CanReopen(now)` checks `Status == ClaimStatus.Rejected`, no prior `ClaimAction.Reopen` in `_history`, and a `ClaimAction.Reject` entry in `_history` with `now - rejectedAt <= TimeSpan.FromDays(ReopenWindowDays)`).
2. **Application & Persistence (`ClaimFlow.Claims`)**:
   - `ClaimPermissions.cs`: Update `AllowedActions(Claim claim, ClaimsPrincipal user, DateTimeOffset now)` to pass `now` to `claim.AllowedActionsFor(user.ToActor(), now)`.
   - `GetClaim.cs`: Inject `TimeProvider timeProvider` into `HandleAsync` and pass `timeProvider.GetUtcNow()` to `ClaimPermissions.AllowedActions`.
   - `ApplyClaimAction.cs`: Map `ClaimAction.Reopen => claim.Reopen(actor, request.Reason, now)` in the `switch` expression and pass `now` to `ClaimPermissions.AllowedActions(claim, user, now)`.
   - `ClaimsSeeder.cs`: Add one sample claim rejected ~10 days ago (e.g., `DaysAgo = 10`, `[ClaimAction.StartReview, ClaimAction.Reject]`) so a fresh start shows a rejected claim within the 30-day reopen window alongside the existing 90-day-old rejected claim (`POL-509114`).
3. **Frontend (`web/`)**:
   - `web/src/app/claims/claim.models.ts`: Add `'Reopen'` to `ClaimAction`.
   - `web/src/app/claims/claim-labels.ts`: Add `Reopen` to `ACTION_META` (`label`: `"Rouvrir le dossier"`, `hint`: `"Nouvelles pièces reçues dans les 30 jours suivant le refus."`, `needsReason: true`, `needsAmount: false`, `tone: 'primary'`).
   - `web/src/app/shared/problem-details.ts`: Add localized French messages for `'claim.reopen_window_expired'` (`@@error.reopenWindowExpired`) and `'claim.already_reopened'` (`@@error.alreadyReopened`).
   - `web/tools/translations.en.json` & `web/src/locale/messages.en.xlf`: Add `"action.reopen": "Reopen"`, `"action.reopen.hint": "New evidence received within 30 days of rejection."`, `"error.reopenWindowExpired"`, and `"error.alreadyReopened"`, then run `npm --prefix web run i18n`.
4. **Architecture Decision Record (`docs/adr/`)**:
   - Write `docs/adr/0010-time-dependent-allowed-actions.md` explaining why `AllowedActionsFor` takes `DateTimeOffset now` from `TimeProvider` and why reopen eligibility is derived from `_history` without extra columns or client-side date logic, and link it in `docs/adr/README.md`.

## Estate Impact *(Gradera)*

*(Note: Direct calls to `gradera/get_impact` during this step returned `session id names no open session`; the impact below is preserved from the earlier `get_context_object` graph edges and direct repository symbol traversal.)*

### What the change reaches

- **Direct anchors**:
  - `src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs`:
    - Reaches `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimEnums.cs` (`ClaimAction`, `ClaimStatus`)
    - Reaches `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs` (`ReasonRequired`, `TransitionNotAllowed`, plus new `ReopenWindowExpired`, `AlreadyReopened`)
    - Reaches `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimStatusChange.cs` (`_history` entries)
    - Reaches `src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs` (`ClaimPermissions.AllowedActions` calling `claim.AllowedActionsFor`)
    - Reaches `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs` and `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimBuilder.cs`
  - `src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs`:
    - Reaches `src/Modules/Claims/ClaimFlow.Claims/Features/GetClaim.cs` (both call `ClaimPermissions.AllowedActions`)
    - Reaches `src/Modules/Claims/ClaimFlow.Claims/Contracts/ClaimRequests.cs` & `ClaimResponses.cs`
    - Reaches `src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsSeeder.cs`
    - Reaches `tests/ClaimFlow.IntegrationTests/ClaimsApiTests.cs` & `SecurityTests.cs`
  - `web/src/app/claims/claim-labels.ts`:
    - Reaches `web/src/app/claims/claim.models.ts` (`ClaimAction` union typed as keys of `ACTION_META`)
    - Reaches `web/src/app/pages/claim-detail/decision-panel.ts`, `decision-panel.html`, and `decision-panel.spec.ts`
    - Reaches `web/src/app/shared/problem-details.ts`, `web/tools/translations.en.json`, and `web/src/locale/messages.en.xlf`
- **Governed rules that apply**:
  - `ko_b31a` (`gradera:b31a7f0c-4be0-409b-9ce4-f54c141aa429`, approved) — Overridden on purpose to add `Reopen` from `Rejected` to `UnderReview`.
  - `ko_8b9d` (`gradera:8b9d4e71-e5df-4eca-907d-68e211227613`, approved) — Overridden on purpose in the portal for eligible `Rejected` claims within 30 days.
  - `ko_2246` (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved) — Preserved (`Settled` permits no actions).
  - `ko_c7c7` (`gradera:c7c74113-1b8b-46f8-bc31-3df9ea3fa38e`, approved) — Preserved (four-eyes settlement rule).
  - `ko_3aff` (`gradera:3aff2147-43bf-44d2-b1fd-f0c78b8a1041`, approved) — Preserved (delegated approval authority limits).
  - `ko_f479` (`gradera:f479f3df-d99f-4370-bd16-04f77f4cb3d2`, approved) — Preserved (2-year incident time bar).
  - `ko_c51b` (`gradera:c51b3bd5-667d-41c4-8351-8827109e0ac0`, approved) — Preserved (rejection transition and mandatory reason).

### Kit skills this plan follows

| Skill | Path | Used by |
|---|---|---|
| `define-domain-errors` | `.github/skills/define-domain-errors/SKILL.md` | US1 (`ClaimErrors.ReopenWindowExpired`, `ClaimErrors.AlreadyReopened`) |
| `add-workflow-transition` | `.github/skills/add-workflow-transition/SKILL.md` | US1 (`ClaimAction.Reopen`, `Claim.Reopen`, `AllowedActionsFor`, `ApplyClaimAction`, `GetClaim`, `ClaimPermissions`) |
| `test-domain-entities` | `.github/skills/test-domain-entities/SKILL.md` | US1 (`ClaimWorkflowTests`, `ClaimBuilder`) |
| `add-claim-action` | `.github/skills/add-claim-action/SKILL.md` | US1 (`claim.models.ts`, `claim-labels.ts`, `decision-panel.spec.ts`) |
| `map-api-error` | `.github/skills/map-api-error/SKILL.md` | US1 (`problem-details.ts`) |
| `add-localized-ui-string` | `.github/skills/add-localized-ui-string/SKILL.md` | US1 (`translations.en.json`, `messages.en.xlf`) |

### Unknowns the engine reported

- None remaining: all unknowns and criteria under "Criteria to confirm" in `KED-28` are resolved.
