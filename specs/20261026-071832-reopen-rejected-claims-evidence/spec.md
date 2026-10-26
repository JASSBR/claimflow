# Feature Specification: Reopen Rejected Claims with New Evidence

**Feature Branch**: `20261026-071832-reopen-rejected-claims-evidence`

**Created**: 2026-10-26

**Status**: Approved

**Input**: Backlog epic "Reopen Rejected Claims with New Evidence" (`KED-27`, `gradera:3764f644-146e-434b-8f4d-ba322fd859df`, approved) and story "Reopen a rejected claim within 30 days" (`KED-28`, `gradera:1d0ef4a0-6281-4eb5-80b1-e75b245e8373`, approved), drafted by the Gradera Context Engine from its stories, acceptance criteria and the estate of `git:claimflow`

## Clarifications

### Session 2026-10-26

- Q: Which closed statuses may be reopened? → A: Only `Rejected` claims in the claims portal may be reopened; `Settled` claims stay permanently closed with no allowed actions (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved). Adjudication engine statuses (`Paid`, `Withdrawn`) are out of scope.
- Q: How is the 30-day reopening window measured and enforced? → A: The window is measured from the rejection timestamp (`OccurredAt`) of the most recent `Reject` entry in the claim's history compared against `TimeProvider` current UTC time (`now - rejectedAt <= TimeSpan.FromDays(30)`): up to and including 30 full days (`<= 30 days`) is allowed and `Reopen` is included in `allowedActions`; after 30 days (`> 30 days`, i.e., day 31 onward) `Reopen` is omitted from `allowedActions` and any direct reopen attempt is refused.
- Q: Which roles are permitted to reopen a rejected claim? → A: Claims handlers (`claims.handler`) and claims managers (`claims.manager`) may reopen eligible rejected claims; auditors (`claims.auditor`) are read-only and refused with HTTP 403 Forbidden.
- Q: What input and evidence are required when reopening, and how many times may a claim be reopened? → A: A non-empty, non-whitespace reason describing the new evidence is required (new documents themselves are uploaded through the existing documents panel with no new document type). A claim may be reopened at most once in its lifecycle; if it was previously reopened and subsequently rejected again, `Reopen` is not offered and any second reopen attempt is refused.
- Q: What happens upon reopening and how is the action presented in the UI? → A: Reopening transitions the claim from `Rejected` to `UnderReview`, appends a `ClaimStatusChange` history entry with the actor, reason, and timestamp, emits a `ClaimStatusChanged` outbox/SignalR live update, and leaves the 2-year incident time bar, handler approval authority limits, and four-eyes settlement rules unchanged. The decision panel renders a Reopen button (`"Rouvrir le dossier"` in French, `"Reopen"` in English) that prompts for the mandatory reason; policyholder notifications are out of scope.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reopen a rejected claim within 30 days (Priority: P1)

As a claims handler, I want to reopen a `Rejected` claim into `UnderReview` status within 30 days of its rejection, so that I can process the claim again when new evidence arrives. (`KED-28`, `gradera:1d0ef4a0-6281-4eb5-80b1-e75b245e8373`, approved)

**Why this priority**: Ordered as the backlog ranks it (`Priority: must`, `Size: M`).

**Independent Test**: Can be fully tested by the acceptance scenarios below across domain unit tests, PostgreSQL integration tests, and Angular Vitest component tests.

**Acceptance Scenarios**:

1. *Settled claims cannot be reopened*: **Given** a claim in `Settled` status, **When** the claims handler views the claim, **Then** no action is permitted (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved).
2. *Reopen option available within 30 days*: **Given** a claim in `Rejected` status (not previously reopened) where the rejection occurred 30 days ago or less (`now - rejectedAt <= 30 days`), **When** a claims handler or manager views the claim, **Then** the `Reopen` option is offered in `allowedActions` (confirmed from `KED-28`).
3. *Reopen option removed from day 31*: **Given** a claim in `Rejected` status where the rejection occurred more than 30 days ago (day 31 onward, `now - rejectedAt > 30 days`), **When** the claims handler views the claim or attempts to reopen it, **Then** the `Reopen` option is not offered and any reopen attempt is refused with `claim.reopen_window_expired` (confirmed from `KED-28`).
4. *Successful reopening transitions to UnderReview*: **Given** a claim in `Rejected` status with the reopen option available, **When** the claims handler reopens the claim with a non-empty reason, **Then** the claim transitions to `UnderReview` status and follows the normal review workflow (`RequestInformation`, `Approve`, `Reject`) (confirmed from `KED-28`).
5. *Auditor cannot reopen a claim*: **Given** a claim in `Rejected` status and an authenticated user with the `claims.auditor` role, **When** the user views the claim or attempts to reopen it, **Then** `Reopen` is not offered and any reopen request is refused with HTTP 403 Forbidden (confirmed from `KED-28`).
6. *Reopen without a reason is refused*: **Given** a claim in `Rejected` status eligible for reopening, **When** a claims handler attempts to reopen the claim without providing a non-whitespace reason, **Then** the system refuses the reopen request with `claim.reason_required` (confirmed from `KED-28`).
7. *Claim reopened once cannot be reopened again*: **Given** a claim that was previously reopened once (`History` already contains a `Reopen` transition) and subsequently rejected again, **When** a claims handler views the claim or attempts to reopen the claim a second time, **Then** `Reopen` is not offered and the reopen request is refused with `claim.already_reopened` (confirmed from `KED-28`).
8. *Reopening records history and live update*: **Given** an eligible rejected claim and an authenticated claims handler or manager providing a reason, **When** the user reopens the claim, **Then** the system records a `ClaimStatusChange` history entry (`Rejected` → `UnderReview`, action `Reopen`, user, trimmed reason, timestamp) and emits a `ClaimStatusChanged` realtime SignalR update via the transactional outbox (confirmed from `KED-28`).

---

### Edge Cases

- **Boundary at Day 30 vs Day 31**: A rejection at `T` can be reopened when `now <= T + TimeSpan.FromDays(30)` (including exactly 30 days to the second); any timestamp `now > T + TimeSpan.FromDays(30)` (e.g., 30 days + 1 tick, or 31 days) is outside the window, omitted from `allowedActions`, and rejected with `claim.reopen_window_expired`.
- **Blank or Whitespace-Only Reason**: Submitting `null`, `""`, or `"   "` as the reopen reason fails validation with `claim.reason_required` (HTTP 400 ValidationProblem) without changing claim status or version.
- **Second Reopen After Re-Rejection**: Once a claim's history contains a `ClaimAction.Reopen` entry, a subsequent rejection is final—`AllowedActions` omits `Reopen`, and invoking `Reopen` returns `claim.already_reopened` (HTTP 409 Conflict).
- **Optimistic Concurrency on Reopen**: Two handlers reopening or mutating the same claim with the same `expectedVersion` results in one succeeding and the second failing with `claim.concurrent_update` (HTTP 409 Conflict), prompting the UI to reload the claim.
- **Unchanged Downstream Governance**: After a claim is reopened into `UnderReview`, existing delegated approval authority limits (`claim.approval_limit_exceeded`), four-eyes settlement rules (`claim.four_eyes_violation`), and the 2-year declaration time bar (`claim.incident_time_barred`) continue to apply unchanged.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST NOT permit any workflow action on a claim in `Settled` status (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved).
- **FR-002**: System MUST support a `Reopen` workflow action (`ClaimAction.Reopen`) that transitions a claim from `Rejected` to `UnderReview` in the `Claim` workflow table.
- **FR-003**: System MUST allow reopening a `Rejected` claim only within 30 days of the most recent rejection timestamp recorded in the claim's history (`now - rejectedAt <= TimeSpan.FromDays(30)`), evaluated using `TimeProvider`, and refuse attempts after 30 days with error code `claim.reopen_window_expired`.
- **FR-004**: System MUST allow a claim to be reopened at most once in its lifecycle; if the claim's history already contains a `Reopen` action, `Reopen` MUST NOT be offered in `allowedActions` and any reopen attempt MUST fail with error code `claim.already_reopened`.
- **FR-005**: System MUST require a non-null, non-whitespace reason describing the new evidence when reopening a claim, trimming leading/trailing whitespace and failing with `claim.reason_required` when missing.
- **FR-006**: System MUST restrict the `Reopen` action to writer roles (`claims.handler` and `claims.manager`) and refuse `claims.auditor` callers with HTTP 403 Forbidden.
- **FR-007**: System MUST compute `allowedActions` on `GET /api/claims/{id}` and `POST /api/claims/{id}/actions` using `TimeProvider` so `Reopen` is included if and only if the caller has permission and the claim is currently `Rejected`, has never been reopened before, and is within 30 days of rejection.
- **FR-008**: System MUST record an immutable `ClaimStatusChange` audit entry (`From = Rejected`, `To = UnderReview`, `Action = Reopen`, actor, reason, timestamp) and raise a `ClaimStatusChanged` domain event dispatched through the transactional outbox to SignalR on every successful reopen.
- **FR-009**: The web portal decision panel MUST display a Reopen action button (`"Rouvrir le dossier"` in French, `"Reopen"` in English) when `Reopen` is in `allowedActions`, prompt for the mandatory reason before submitting, and localize any reopen domain error codes in both French and English.
- **FR-010**: Existing domain rules for the 2-year incident time bar (`gradera:f479f3df-d99f-4370-bd16-04f77f4cb3d2`, approved), handler approval authority limits (`gradera:3aff2147-43bf-44d2-b1fd-f0c78b8a1041`, approved), and four-eyes settlement (`gradera:c7c74113-1b8b-46f8-bc31-3df9ea3fa38e`, approved) MUST remain unchanged after a claim is reopened.

### Key Entities

- **`Claim` (`ClaimFlow.Claims.Domain.Claim`)**: Aggregate root owning the workflow table, status, history collection (`IReadOnlyList<ClaimStatusChange>`), `Reopen(Actor, string?, DateTimeOffset)` domain method, and `AllowedActionsFor(Actor, DateTimeOffset)` eligibility evaluation.
- **`ClaimAction` (`ClaimFlow.Claims.Domain.ClaimAction`)**: Workflow action enum extended with `Reopen`.
- **`ClaimStatusChange` (`ClaimFlow.Claims.Domain.ClaimStatusChange`)**: Immutable audit trail entry recording `From`, `To`, `Action`, `ActorId`, `ActorName`, `Reason`, and `OccurredAt` (used to determine prior `Reopen` count and the latest `Reject` timestamp).

## Out of Scope

- Reopening claims in `Settled` status.
- Reopening claims in the adjudication engine (`Paid` and `Withdrawn` statuses).
- Introducing new document types or mandatory document-upload gates on the reopen command (new evidence is attached via the existing documents panel and described in the reopen reason).
- Sending external notifications (e.g., email/SMS) to the policyholder upon reopening.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 8 acceptance scenarios of User Story 1 pass in domain unit tests (`ClaimFlow.Claims.Domain.UnitTests`), Testcontainers PostgreSQL integration tests (`ClaimFlow.IntegrationTests`), and Angular Vitest specs (`web/`).
- **SC-002**: `dotnet build ClaimFlow.slnx` finishes with 0 warnings and 0 errors, `dotnet format ClaimFlow.slnx --verify-no-changes` is clean, and all French/English translations build cleanly via `npm --prefix web run i18n` and `npm --prefix web run build`.

## Assumptions

- New evidence is any document the handler attaches to the claim through the existing documents panel; no new document type is required, and the mandatory reopen reason states what arrived.
- No database schema migration is required because `ClaimStatusChange.Action` is stored as text (`HasConversion<string>()`) and both the prior reopen check and the 30-day rejection window are derived directly from the claim's persisted `_history` collection (`ClaimStatusChange` rows).
- Scope is the backlog epic `KED-27` and story `KED-28` with all 7 business-confirmed criteria integrated.

## Estate Context *(Gradera)*

### Constraints that bind this feature

- **Overridden on purpose (acknowledged)**: `CH-1` and `AC-1.1` allow offering and executing `Reopen` on an eligible `Rejected` claim to transition it back to `UnderReview`, intentionally overriding `ko_b31a` (`gradera:b31a7f0c-4be0-409b-9ce4-f54c141aa429`, approved, `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143`), which previously specified no allowed actions for `Rejected` claims.
- **Overridden on purpose (acknowledged)**: `AC-1.3` transitions a `Rejected` claim in the claims portal back to `UnderReview` within 30 days, overriding `ko_8b9d` (`gradera:8b9d4e71-e5df-4eca-907d-68e211227613`, approved) to the extent that `Rejected` in the claims portal can be reopened once within 30 days while adjudication engine statuses remain untouched.
- **Preserved constraints**:
  - `Settled` claims permit no actions (`gradera:2246c819-8075-4173-a11d-4187dc882147`, approved).
  - Four-eyes principle at settlement (`gradera:c7c74113-1b8b-46f8-bc31-3df9ea3fa38e`, approved).
  - Delegated approval authority limits (`gradera:3aff2147-43bf-44d2-b1fd-f0c78b8a1041`, approved).
  - Two-year incident time bar under Code des assurances L114-1 (`gradera:f479f3df-d99f-4370-bd16-04f77f4cb3d2`, approved).
  - Rejecting a claim requires a non-whitespace reason (`gradera:c51b3bd5-667d-41c4-8351-8827109e0ac0`, approved).

### Estate facts to confirm

- None: all 7 criteria under "Criteria to confirm" in `KED-28` have been settled by the business and integrated into User Story 1 and FR-001 through FR-010.
