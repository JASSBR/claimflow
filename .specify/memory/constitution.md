<!--
Sync Impact Report
- Version change: N/A (template placeholders) → 1.0.0
- Modified principles:
  - [PRINCIPLE_1_NAME] → I. Single-Source Workflow & Server-Driven UI
  - [PRINCIPLE_2_NAME] → II. Test-First Discipline (NON-NEGOTIABLE)
  - [PRINCIPLE_3_NAME] → III. Estate Knowledge Governance
  - [PRINCIPLE_4_NAME] → IV. Explicit Error Handling & Deterministic Time
  - [PRINCIPLE_5_NAME] → V. Bilingual UI Localization (FR / EN)
  - Added VI. Audited State Changes & Transactional Realtime Outbox
  - Added VII. Zero-Warning Quality Gate
- Added sections:
  - Architecture & Security Constraints
  - Development Workflow & Verification Gates
- Removed sections: None
- Follow-up TODOs: None
-->
# ClaimFlow Constitution

## Core Principles

### I. Single-Source Workflow & Server-Driven UI
The workflow table in `Claim.cs` MUST be the single source of truth for claim status transitions.
The server computes `allowedActions` as the intersection of workflow rules, caller role
permissions (`ClaimPermissions`), and domain invariants such as four-eyes segregation of duties
(`(gradera:c7c74113-1b8b-46f8-bc31-3df9ea3fa38e, approved)`,
`(gradera:fe0fcd61-8fd8-4a15-95b6-40903c8aa6ab, approved)`). The Angular UI MUST render only the
actions returned by the server in `allowedActions` and MUST NOT duplicate or infer workflow
transition rules client-side (ADR 0007, ADR 0008).

### II. Test-First Discipline (NON-NEGOTIABLE)
Every backend change MUST follow the Red-Green-Refactor cycle in strict order:
1. Domain unit test (`ClaimFlow.Claims.Domain.UnitTests` / `ClaimFlow.Documents.UnitTests` with
   xUnit v3 and Shouldly).
2. Integration test against a real PostgreSQL instance via Testcontainers
   (`ClaimFlow.IntegrationTests` — never the EF Core in-memory provider, ADR 0006).
3. Production implementation.

Every UI change in `web/` MUST include or update a corresponding Vitest spec (`*.spec.ts`)
verifying component behavior, HTTP requests, and reactive state.

### III. Estate Knowledge Governance
Approved knowledge in the Gradera Knowledge Engine is the authoritative source of truth for
existing system behavior (`(gradera:b31a7f0c-4be0-409b-9ce4-f54c141aa429, approved)`,
`(gradera:c51b3bd5-667d-41c4-8351-8827109e0ac0, approved)`,
`(gradera:3aff2147-43bf-44d2-b1fd-f0c78b8a1041, approved)`,
`(gradera:f479f3df-d99f-4370-bd16-04f77f4cb3d2, approved)`). Code changes MUST NOT contradict an
`approved` or `validated` estate rule unless the feature specification explicitly documents and
acknowledges the override. Unconfirmed behavior (`inferred` or labelled `criteria-to-confirm`)
MUST be surfaced and confirmed in the specification before implementation.

### IV. Explicit Error Handling & Deterministic Time
Expected business and validation failures MUST be returned as `Result` / `Result<T>` values with
structured `Error` codes defined in `ClaimErrors` / `DocumentErrors` and mapped to RFC 9457
`ProblemDetails` responses via `ResultExtensions.ToProblem()` (ADR 0005). Exceptions are reserved
strictly for unexpected defects. All current-time reads in application and domain logic MUST come
from injected `TimeProvider` (passed into domain methods as `DateTimeOffset now` or `DateOnly
today`) rather than `DateTime.UtcNow` or `DateTimeOffset.UtcNow`.

### V. Bilingual UI Localization (FR / EN)
Every user-facing string in the Angular application MUST exist in both French (source locale) and
English (`messages.en.xlf`). Templates and components MUST use `i18n="@@id"` attributes or
`$localize` with stable message IDs, and API error codes MUST be mapped to localized client-side
messages in `web/src/app/shared/problem-details.ts`. Any UI text change MUST run `npm --prefix web
run i18n` so extraction and translation verification succeed with zero missing translations
(ADR 0007).

### VI. Audited State Changes & Transactional Realtime Outbox
Every aggregate state change MUST execute through a domain method on the aggregate root, append an
immutable history entry (`ClaimStatusChange`), and raise a domain event registered in
`AddOutbox<TDbContext>()` (ADR 0002). `DomainEventsToOutboxInterceptor` persists outbox messages in
the same database transaction as the state transition, and `OutboxProcessor` dispatches them to
publish a SignalR `claimChanged` realtime notification. Write commands MUST enforce optimistic
concurrency via `expectedVersion` mapped to PostgreSQL's `xmin` system column (ADR 0003).

### VII. Zero-Warning Quality Gate
A task is complete only when all quality checks pass cleanly:
- `dotnet build ClaimFlow.slnx` completes with 0 warnings and 0 errors (never suppress warnings
  with `#pragma`).
- `dotnet test --solution ClaimFlow.slnx` passes all unit, architecture, and integration tests.
- `dotnet format ClaimFlow.slnx --verify-no-changes` reports no formatting diffs.
- `npm --prefix web run format:check`, `npm --prefix web run lint`, `npm --prefix web run test`,
  and `npm --prefix web run build` all succeed cleanly.

## Architecture & Security Constraints

- **Modular Monolith Boundaries**: `ClaimFlow.Api` hosts isolated vertical modules (`Claims` and
  `Documents`), each owning its PostgreSQL schema, `DbContext`, migrations, and outbox table.
  Cross-module calls MUST go through public `Contracts` (`IClaimDirectory`), never internal types
  or tables, enforced by `ClaimFlow.ArchitectureTests` (ADR 0001).
- **Pure Domain Layer**: `*.Domain` projects MUST depend only on `ClaimFlow.SharedKernel` with no
  references to EF Core, ASP.NET Core, or `ClaimFlow.BuildingBlocks` (ADR 0001).
- **Direct `DbContext` Usage**: Feature handlers use the module `DbContext` directly without a
  generic repository wrapper (ADR 0004).
- **Segregation of Duties & Authority Limits**: Role policies (`claims.read`, `claims.write`) and
  domain rules (handler approval limits and four-eyes settlement prohibition) MUST both be enforced
  on every command (`(gradera:5f0275b6-3e8f-49bd-b1f3-d33945e63656, approved)`,
  `(gradera:bf67a5e4-addb-4632-b736-6226c46cc895, approved)`, ADR 0008).
- **Frontend Reactivity**: All Angular components MUST use `ChangeDetectionStrategy.OnPush`,
  `inject()`, `httpResource` for reads, Signal Forms for forms, and `reloadWhen(trigger, resource)`
  for realtime updates without resetting resources to an empty loading state (ADR 0007).

## Development Workflow & Verification Gates

1. **Ground in the Estate & ADRs**: Check governed rules in the Gradera Knowledge Engine, component
   instructions in `.github/instructions/`, and relevant skills in `.github/skills/<name>/SKILL.md`
   before planning or coding. Record architectural decisions in `docs/adr/` when introducing a
   design choice that could be challenged.
2. **Test-First Execution**: Write failing domain unit tests first, followed by failing
   Testcontainers integration tests and Vitest specs, before implementing production code.
3. **Full Verification Loop**: Run `dotnet build ClaimFlow.slnx`, `dotnet test --solution
   ClaimFlow.slnx`, `dotnet format ClaimFlow.slnx --verify-no-changes`, and the `web/` scripts
   (`i18n`, `format:check`, `lint`, `test`, `build`) prior to completion.

## Governance

This constitution supersedes ad-hoc practices across the `claimflow` repository. Amendments require:
1. Updating `.specify/memory/constitution.md` via `/speckit.constitution` with an explicit rationale
   and citations to ADRs or approved Gradera Knowledge Engine objects.
2. Incrementing the constitution version according to Semantic Versioning:
   - **MAJOR**: Removal or backward-incompatible redefinition of an existing core principle.
   - **MINOR**: Addition of a new core principle or governance section, or material expansion of
     existing rules.
   - **PATCH**: Wording clarifications, citation updates, or non-semantic refinements.
3. Verifying all specifications, implementation plans, tasks, and pull requests against these
   principles and the automated verification gates.

**Version**: 1.0.0 | **Ratified**: 2026-10-02 | **Last Amended**: 2026-10-26
