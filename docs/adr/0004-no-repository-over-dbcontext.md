# 0004 — No repository layer over `DbContext`

- **Status:** Accepted · 2026-10-02
- **Deviates from:** ECC `rules/csharp/patterns.md` ("Repository Pattern")

## Context

The generic `IRepository<T>` (`FindAll`, `FindById`, `Create`, `Update`, `Delete`) is the default in many codebases.
EF Core's `DbContext` already **is** a unit of work and `DbSet<T>` already **is** a repository.

## Decision

Feature handlers use `ClaimsDbContext` directly. Read endpoints project straight to response DTOs with
`AsNoTracking().Select(...)`; write endpoints load the aggregate, call a domain method, and save.

## Consequences

- ✅ Queries keep EF's full power (projection, `GroupBy` translated to SQL, split queries) instead of being flattened
  behind `FindAll()` and filtered in memory.
- ✅ One less layer to read and to mock; behaviour is tested against a real database instead ([0006](0006-testing-strategy.md)).
- ⚠️ Handlers depend on EF Core. They live in the module project, which is allowed to — the **domain** project is not,
  and `ArchitectureTests` enforce it.
- 🔁 Revisit if a module needs a non-relational store, or if the same complex query appears in three places
  (then extract a query object, not a generic repository).
