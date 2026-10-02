# Architecture Decision Records

Each record captures one decision, the context that forced it, and what it costs. Format: [MADR-lite](https://adr.github.io/madr/).

| # | Decision | Status |
|---|----------|--------|
| [0001](0001-modular-monolith.md) | Modular monolith, not microservices | Accepted |
| [0002](0002-transactional-outbox.md) | Transactional outbox for domain events | Accepted |
| [0003](0003-postgresql-optimistic-concurrency.md) | PostgreSQL + EF Core, optimistic concurrency on `xmin`, migrations as a bundle | Accepted |
| [0004](0004-no-repository-over-dbcontext.md) | No repository layer over `DbContext` | Accepted |
| [0005](0005-result-pattern-problem-details.md) | Result pattern + RFC 9457 problem details | Accepted |
| [0006](0006-testing-strategy.md) | Testing strategy: real PostgreSQL, enforced architecture | Accepted |
| [0007](0007-frontend-signals-server-driven-workflow.md) | Zoneless Angular, signals end-to-end, server-driven workflow | Accepted |
