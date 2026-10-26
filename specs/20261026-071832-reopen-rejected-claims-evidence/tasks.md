---

description: "Task list for Reopen Rejected Claims with New Evidence, drafted from the backlog and refined plan"
---

# Tasks: Reopen Rejected Claims with New Evidence

**Input**: Design documents from `/specs/20261026-071832-reopen-rejected-claims-evidence/` (`spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/reopen-claim.md`, `quickstart.md`)

**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories)

**Tests**: Included and mandatory (Constitution Principle II). Every acceptance scenario in `spec.md` is written as a failing test first (`ClaimFlow.Claims.Domain.UnitTests` → `ClaimFlow.IntegrationTests` → Vitest `web/`), before production implementation.

**Organization**: Tasks are grouped by phase with one phase per user story, preserving the engine's citations on test tasks and naming the promoted kit skill (`.github/skills/<name>/SKILL.md`) each task follows.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (`[US1]`)

## Phase 1: Setup (Shared Infrastructure)

- [X] T001 Confirm every NEEDS CLARIFICATION in `specs/20261026-071832-reopen-rejected-claims-evidence/spec.md` and `specs/20261026-071832-reopen-rejected-claims-evidence/plan.md` with `/speckit.clarify` and `/speckit.plan`
- [X] T002 [P] Read the impact of `src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs`, `src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs`, and `web/src/app/claims/claim-labels.ts` and record in `specs/20261026-071832-reopen-rejected-claims-evidence/plan.md`

---

## Phase 2: User Story 1 - Reopen a rejected claim within 30 days (Priority: P1) 🎯 MVP

**Goal**: As a claims handler, I want to reopen a `Rejected` claim into `UnderReview` status within 30 days of its rejection, so that I can process the claim again when new evidence arrives (`KED-28`, `gradera:1d0ef4a0-6281-4eb5-80b1-e75b245e8373`, approved).

**Independent Test**: All 8 acceptance scenarios of User Story 1 in `specs/20261026-071832-reopen-rejected-claims-evidence/spec.md` pass across `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs`, `tests/ClaimFlow.IntegrationTests/ClaimsApiTests.cs`, and `web/src/app/pages/claim-detail/decision-panel.spec.ts`.

### Tests for User Story 1 (Write First — Must Fail Before Implementation) ⚠️

- [X] T003 [P] [US1] Write domain unit tests in `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs` (and helper `Rejected` in `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimBuilder.cs`) following skill `test-domain-entities` (`.github/skills/test-domain-entities/SKILL.md`) covering:
  - `Settled` claims cannot be reopened and have no allowed actions (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved; cites `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143`, `ko_b31a` `gradera:b31a7f0c-4be0-409b-9ce4-f54c141aa429`, approved)
  - `Reopen` allowed within 30 days of rejection, including boundary at exactly day 30 (`now = rejectedAt.AddDays(30)`) transitioning `Rejected` → `UnderReview`, recording a `ClaimStatusChange` history entry (actor, trimmed reason, timestamp), and raising `ClaimStatusChanged`
  - `Reopen` refused from day 31 (`now = rejectedAt.AddDays(31)` and `rejectedAt.AddDays(30).AddTicks(1)`) with `ClaimErrors.ReopenWindowExpired` (`claim.reopen_window_expired`, `ErrorType.Conflict`)
  - `Reopen` without a non-whitespace reason (`null`, `""`, `"   "`) refused with `ClaimErrors.ReasonRequired` (`claim.reason_required`, `ErrorType.Validation`)
  - Second reopen after a claim was already reopened once and subsequently rejected again refused with `ClaimErrors.AlreadyReopened` (`claim.already_reopened`, `ErrorType.Conflict`)
  - `AllowedActionsFor(actor, now)` includes `ClaimAction.Reopen` up to day 30 on a first rejection and hides `ClaimAction.Reopen` after day 30 (`now > rejectedAt.AddDays(30)`) and after one reopen
- [X] T004 [P] [US1] Write PostgreSQL integration tests in `tests/ClaimFlow.IntegrationTests/ClaimsApiTests.cs` (and role check in `tests/ClaimFlow.IntegrationTests/SecurityTests.cs` if needed) covering `POST /api/claims/{id}/actions` with `ClaimAction.Reopen`:
  - `200 OK` returning `ClaimDetailsResponse` in `UnderReview` status with the `Reopen` history entry and `RequestInformation`, `Approve`, `Reject` in `allowedActions`
  - `403 Forbidden` when an authenticated auditor (`sophie`, `claims.auditor`) attempts to `Reopen` (`gradera:fe0fcd61-8fd8-4a15-95b6-40903c8aa6ab`, approved)
  - ProblemDetails refusals with exact codes: `400 Bad Request` (`claim.reason_required`) when reason is missing, `409 Conflict` (`claim.reopen_window_expired`) when rejection in history is older than 30 days (using seeded 90-day rejected claim or fake `TimeProvider`), `409 Conflict` (`claim.already_reopened`) on a second reopen after re-rejection, and `409 Conflict` (`claim.transition_not_allowed`) on a `Settled` claim (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved)
- [X] T005 [P] [US1] Write Vitest component unit test in `web/src/app/pages/claim-detail/decision-panel.spec.ts` verifying that when `allowedActions` contains `'Reopen'`, `DecisionPanel` renders the `"Rouvrir le dossier"` button, opens the reason textarea when clicked, and emits `{ action: 'Reopen', reason: '...', approvedAmount: undefined }` on confirm, following skill `add-claim-action` (`.github/skills/add-claim-action/SKILL.md`)

### Implementation for User Story 1

- [X] T006 [US1] Add `ClaimErrors.ReopenWindowExpired` (`Error.Conflict("claim.reopen_window_expired", "A rejected claim can only be reopened within 30 days of rejection.")`) and `ClaimErrors.AlreadyReopened` (`Error.Conflict("claim.already_reopened", "A claim can only be reopened once.")`) in `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs`, following skill `define-domain-errors` (`.github/skills/define-domain-errors/SKILL.md`)
- [X] T007 [US1] Add `ClaimAction.Reopen` to `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimEnums.cs` and implement `[ClaimAction.Reopen] = ([ClaimStatus.Rejected], ClaimStatus.UnderReview)`, `ReopenWindowDays = 30`, `Claim.Reopen(Actor actor, string? reason, DateTimeOffset now)`, and time-aware `Claim.AllowedActionsFor(Actor actor, DateTimeOffset now)` in `src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs`, following skill `add-workflow-transition` (`.github/skills/add-workflow-transition/SKILL.md`)
- [X] T008 [US1] Update `ClaimPermissions.AllowedActions(Claim claim, ClaimsPrincipal user, DateTimeOffset now)` in `src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs`, pass `timeProvider.GetUtcNow()` in `src/Modules/Claims/ClaimFlow.Claims/Features/GetClaim.cs`, and dispatch `ClaimAction.Reopen => claim.Reopen(actor, request.Reason, now)` in `src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs`, following skill `add-workflow-transition` (`.github/skills/add-workflow-transition/SKILL.md`)
- [X] T009 [P] [US1] Add a sample claim rejected ~10 days ago (`DaysAgo = 10`, path `[ClaimAction.StartReview, ClaimAction.Reject]`) in `src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsSeeder.cs` so a fresh start shows a rejected claim with the Reopen button enabled
- [X] T010 [US1] Add `'Reopen'` to `ClaimAction` in `web/src/app/claims/claim.models.ts` and add the `Reopen` entry (`label: $localize`:@@action.reopen:Rouvrir le dossier``, `hint: $localize`:@@action.reopen.hint:Nouvelles pièces reçues dans les 30 jours suivant le refus.``, `needsReason: true`, `needsAmount: false`, `tone: 'primary'`) to `ACTION_META` in `web/src/app/claims/claim-labels.ts`, following skill `add-claim-action` (`.github/skills/add-claim-action/SKILL.md`)
- [X] T011 [US1] Map `'claim.reopen_window_expired'` (`@@error.reopenWindowExpired`) and `'claim.already_reopened'` (`@@error.alreadyReopened`) in `web/src/app/shared/problem-details.ts`, following skill `map-api-error` (`.github/skills/map-api-error/SKILL.md`)
- [X] T012 [US1] Add English translations for `action.reopen` (`"Reopen"`), `action.reopen.hint` (`"New evidence received within 30 days of rejection."`), `error.reopenWindowExpired`, and `error.alreadyReopened` in `web/tools/translations.en.json` and regenerate `web/src/locale/messages.en.xlf` via `npm --prefix web run i18n`, following skill `add-localized-ui-string` (`.github/skills/add-localized-ui-string/SKILL.md`)

**Checkpoint**: User Story 1 is fully implemented and all domain, integration, and Vitest tests pass independently.

---

## Phase 3: Polish & Cross-Cutting Concerns

- [X] T013 [P] Write `docs/adr/0010-time-dependent-allowed-actions.md` documenting why `AllowedActionsFor` takes `DateTimeOffset now` from `TimeProvider` and why the 30-day window and single-reopen rule are derived from `_history`, and add ADR 0010 to the index table in `docs/adr/README.md`
- [X] T014 [P] Update `README.md` if needed to mention reopening rejected claims within 30 days and the 10 architecture decision records
- [X] T015 Run `/speckit.analyze` and verify all quality gates (`dotnet build ClaimFlow.slnx`, `dotnet test --solution ClaimFlow.slnx`, `dotnet format ClaimFlow.slnx --verify-no-changes`, and `npm --prefix web run {i18n,format:check,lint,test,build}`)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Completed (`T001`, `T002`).
- **User Story 1 (Phase 2)**:
  1. Write failing tests first (`T003`, `T004`, `T005` can be authored in parallel across backend and frontend test files).
  2. Implement domain errors and domain transition (`T006` → `T007`).
  3. Wire application permissions, endpoints, and seed sample (`T008`, `T009`).
  4. Implement frontend action metadata, error mappings, and i18n (`T010` → `T011` → `T012`).
- **Polish & Cross-Cutting (Phase 3)**:
  - Write ADR 0010 (`T013`) and update `README.md` (`T014`) in parallel, then run full analysis and verification (`T015`).

### Parallel Opportunities

- `T003` (`ClaimWorkflowTests.cs`), `T004` (`ClaimsApiTests.cs`), and `T005` (`decision-panel.spec.ts`) touch separate test projects and can be written in parallel.
- Backend implementation (`T006`–`T009`) and frontend implementation (`T010`–`T012`) touch disjoint directories (`src/` vs `web/`) and can proceed in parallel once `ClaimAction.Reopen` and error codes are agreed.
- Documentation tasks `T013` (`docs/adr/`) and `T014` (`README.md`) can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Write failing tests across all three test layers:
Task T003: "Write domain unit tests in tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs"
Task T004: "Write PostgreSQL integration tests in tests/ClaimFlow.IntegrationTests/ClaimsApiTests.cs"
Task T005: "Write Vitest component unit test in web/src/app/pages/claim-detail/decision-panel.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (`T001`–`T002` done).
2. Complete Phase 2: User Story 1 (`T003`–`T012`), verifying tests fail before implementation and pass after.
3. Complete Phase 3: ADR 0010, `README.md`, `/speckit.analyze`, and full zero-warning build/test/lint/format verification (`T013`–`T015`).
