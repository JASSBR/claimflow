# 0001 — Modular monolith, not microservices

- **Status:** Accepted · 2026-10-02

## Context

ClaimFlow will grow several bounded contexts: claims handling, documents (RAG on attachments), policies, payments.
One developer, one deployable, a demo budget of zero. Microservices would buy independent scaling and deployment
we don't need, at the price of distributed transactions, network failure modes and N pipelines.

## Decision

One deployable (`ClaimFlow.Api`) composed of **modules**. A module is a vertical slice of the business with:

- its own **domain project** (`ClaimFlow.Claims.Domain`) that depends on `SharedKernel` only;
- its own **PostgreSQL schema** (`claims`), `DbContext`, migrations history table and outbox table;
- a single public entry point (`ClaimsModule.AddClaimsModule` / `MapClaimsModule`) plus a `Contracts` namespace.
  Everything else is `internal`.

Modules talk through contracts and domain events (see [0002](0002-transactional-outbox.md)), never through each other's tables.

## Consequences

- ✅ Boundaries are **enforced by tests**, not by convention: `ArchitectureTests` fail the build if the domain references
  EF Core/ASP.NET Core, if a module leaks a public type, or if the host reaches into a module's internals.
- ✅ Extracting a module into a service later is mechanical: its schema, events and contracts are already isolated.
- ⚠️ All modules share one process: a memory leak or a hot loop in one affects the others. Acceptable at this scale.
