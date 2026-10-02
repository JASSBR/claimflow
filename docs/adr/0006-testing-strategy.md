# 0006 — Testing strategy: real PostgreSQL, enforced architecture

- **Status:** Accepted · 2026-10-02

## Decision

| Layer | Tool | What it proves |
|-------|------|----------------|
| Domain unit tests | xUnit v3 + Shouldly | Workflow table, invariants (L114-1 time bar, amounts, reasons), events raised |
| Architecture tests | NetArchTest | Module boundaries ([0001](0001-modular-monolith.md)) — mutation-checked: making a feature public fails the build |
| Integration tests | `WebApplicationFactory` + **Testcontainers PostgreSQL 18** | Real HTTP pipeline, real SQL: `xmin` concurrency, `SKIP LOCKED` outbox, SignalR push, problem details, security headers |
| Frontend | Vitest (Angular unit-test builder) | Components against `HttpTestingController`, realtime refetch rules, Signal Forms validation |

- **No EF in-memory provider**: it ignores transactions, `xmin`, `jsonb` and raw SQL — exactly what these tests exist to prove.
- Tests run on **Microsoft.Testing.Platform** (the .NET 10 successor of VSTest).
- **Shouldly instead of FluentAssertions**: FluentAssertions v8 moved to a commercial licence; Shouldly is MIT and
  reads as well.
- **Coverage gate: 80 % of production lines** (`eng/coverage-gate.py`, migrations and AppHost excluded) in CI,
  plus Vitest thresholds on the frontend.

## Consequences

- ✅ A green build means the system works on the database it ships with.
- ⚠️ Integration tests need Docker (≈20 s on a laptop, container reused across the test collection).
