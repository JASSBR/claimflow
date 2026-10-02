# 0003 — PostgreSQL + EF Core, optimistic concurrency on `xmin`, migrations as a bundle

- **Status:** Accepted · 2026-10-02

## Context

Two claim handlers can open the same claim; one approves while the other rejects. Last-write-wins would silently
discard a decision on an insurance file — unacceptable for audit. Locking rows for the duration of a human decision
is not an option either.

## Decision

- **PostgreSQL** (via Npgsql EF Core provider): `jsonb` for outbox payloads, partial indexes, `SKIP LOCKED`.
- **Optimistic concurrency on the `xmin` system column** (`Version` mapped with `IsRowVersion()`): no extra column,
  PostgreSQL bumps it on every update.
- The API returns `version` with every claim; a command must send back the `expectedVersion` it was based on.
  `ApplyClaimAction` sets it as the *original* value, so a stale command fails with **409 `claim.concurrent_update`**
  and the UI reloads the fresh state.
- **Migrations**: applied at startup only when `Database:InitializeOnStartup` is true (local, tests, demo).
  For production the CI produces a self-contained **EF migration bundle** (`efbundle` artifact) that runs as a
  pipeline step before the new version receives traffic — the app never needs DDL rights at runtime.

- **Claim numbers** (`SIN-2026-000123`) come from a PostgreSQL sequence (`claims.claim_number_seq`): unique across
  concurrent transactions by construction. A rolled-back declaration leaves a gap, which a business reference tolerates.

## Consequences

- ✅ Lost updates are impossible, proven by `StaleVersion_Returns409_AndDoesNotOverwriteConcurrentDecision`.
- ✅ Enums stored as text, amounts as `numeric(12,2)`: the database is readable by a human and safe against enum reordering.
- ⚠️ `xmin` is PostgreSQL-specific; switching databases means switching to a `rowversion`/token column.
