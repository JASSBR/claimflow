# 0002 — Transactional outbox for domain events

- **Status:** Accepted · 2026-10-02

## Context

When a claim changes state, other parts of the system must react: the realtime dashboard today, e-mail notifications,
the payment module and the audit search index tomorrow. Publishing "after `SaveChanges`" is a **dual write**:
if the process dies between the commit and the publish, the event is lost; if we publish first and the commit fails,
we announce something that never happened.

## Decision

- Aggregates **buffer** domain events (`AggregateRoot.Raise`).
- `DomainEventsToOutboxInterceptor` turns them into `outbox_messages` rows **inside the same transaction** as the state change.
- `OutboxProcessor<TDbContext>` (a `BackgroundService`) claims pending rows with `SELECT … FOR UPDATE SKIP LOCKED`,
  dispatches them to `IDomainEventHandler<T>` implementations, and marks them processed. Failures are retried up to
  5 times with the error stored on the row.
- Event types are resolved through a **closed registry** (`OutboxEventRegistry`), never `Type.GetType(row.Type)`:
  a tampered row cannot make the processor instantiate an arbitrary type.

## Consequences

- ✅ No lost and no phantom events. Proven by `OutboxTests`: the SignalR client receives nothing until the outbox delivers.
- ✅ Several API replicas can poll concurrently (`SKIP LOCKED`), no leader election needed.
- ✅ Throughput and failures are observable: `claimflow.outbox.processed` / `claimflow.outbox.failed` metrics and a
  trace span per dispatch, visible in the Aspire dashboard.
- ⚠️ Delivery is **at-least-once**: handlers must be idempotent (the current one only triggers a UI refresh).
- ⚠️ Latency = polling interval (2 s). Good enough for a back-office; `LISTEN/NOTIFY` is the upgrade path if it isn't.
