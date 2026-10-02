# ClaimFlow — instructions for Claude Code

Portfolio project: insurance claims handling. .NET 10 modular monolith + Angular 22 + PostgreSQL, orchestrated by .NET Aspire.
Goal: production-grade engineering that a tech lead can review. Quality > feature count.

## Commands

```bash
export PATH="$HOME/.dotnet:$PATH"          # SDK is user-level on this machine
dotnet run --project src/ClaimFlow.AppHost  # whole system (needs Docker)
dotnet build ClaimFlow.slnx                 # must stay at 0 warnings
dotnet test --solution ClaimFlow.slnx       # integration tests need Docker
dotnet format ClaimFlow.slnx --verify-no-changes
cd web && nvm use && npm run format:check && npm run lint && npm test && npm run build
cd web && E2E_BASE_URL=<web endpoint of the running AppHost> npm run e2e   # Playwright, full stack
cd web && npm run i18n                     # after any UI text change: re-extract + rebuild messages.en.xlf (fails on a missing translation)
dotnet ef migrations add <Name> --project src/Modules/<Module>/ClaimFlow.<Module> --startup-project src/Modules/<Module>/ClaimFlow.<Module> --output-dir Persistence/Migrations
./deploy/azure.sh                          # API → Azure Container Apps (needs az login); SPA_ORIGIN / ANTHROPIC_API_KEY optional
API_URL=https://… ./deploy/vercel.sh       # SPA → Vercel (FR + EN builds)
```

## ECC setup in this repo

- Rules: `.claude/rules/{common,csharp,angular}` (from affaan-m/ECC). Angular rule paths were retargeted to
  `web/src/**` because Angular 20+ dropped the `.component.ts` suffix the upstream globs expect.
- Agent: `csharp-reviewer` — run it on every backend diff before committing.
- Skills: `dotnet-patterns`, `csharp-testing`, `angular-developer`, plus the global `tdd-workflow`, `verification-loop`, `security-review`.
- Hooks (`.claude/settings.json`): format the edited file after each edit; on Stop, build + lint whatever side changed.

**Where this repo deliberately overrides ECC** (see ADRs): no repository over DbContext (0004), Result + ProblemDetails
instead of an `ApiResponse<T>` envelope (0005), Shouldly instead of FluentAssertions (0006, licence).

## Workflow per feature

1. Write or update an ADR in `docs/adr/` when the change involves a decision someone could challenge.
2. TDD: domain unit test → integration test (real PostgreSQL) → implementation.
3. `csharp-reviewer` on the diff; `/security-review` if auth, input handling or secrets are touched.
4. `/verification-loop`: build, format, tests, coverage gate (`python3 eng/coverage-gate.py 80`), lint.
5. Conventional commit (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`), subject ≤ 70 chars, **no AI attribution**.

## Hard rules

- Modules: **Claims** and **Documents**. A module talks to another only through its public contract
  (`IClaimDirectory` in `ClaimFlow.Claims.Contracts`), never its Domain/Features/Persistence (ArchitectureTests).
- Every UI string goes through `i18n="@@id"` / `$localize` with a stable id, then `npm run i18n`; API errors are worded
  client-side from their code (`shared/problem-details.ts`).
- Realtime refresh: `reloadWhen(trigger, resource)` — never make a resource's request depend on a notification (it resets to an empty loading state).
- AI review: `IClaimAnalyst` is the seam; tests use `FakeClaimAnalyst`, CI never calls the paid API.
- `*.Domain` projects reference `SharedKernel` only — never EF Core, ASP.NET Core or BuildingBlocks (ArchitectureTests enforce it).
- Only `ClaimsModule` and the `Contracts` namespace are public in a module. Features, persistence, realtime stay `internal`.
- Business failures are `Result` values mapped by `ToProblem()`; exceptions are for bugs only.
- Every state change goes through an aggregate method, so it is recorded in history and raises an outbox event.
- Commands take `expectedVersion`; never bypass the `xmin` check.
- New domain events must be registered in `AddOutbox<...>(...)` or SaveChanges throws.
- Angular: OnPush everywhere (ESLint-enforced), `inject()`, `httpResource` for reads, Signal Forms for forms, no workflow logic in the UI (render `allowedActions`).
- Analyzer warning? Fix it. If a rule is genuinely wrong for this codebase, disable it in `.editorconfig` with a one-line justification — never `#pragma`.
- No secrets in `appsettings*.json`: user-secrets locally, platform env vars in Azure.
