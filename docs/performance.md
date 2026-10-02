# Performance

Measured on 2026-10-02 with [k6](https://k6.io) ([`tests/load/claims.js`](../tests/load/claims.js)) against the
**production container image** (Release build, chiseled runtime) — not the development server.

**Setup.** API container limited to **2 vCPU / 1 GB RAM**, PostgreSQL 18 and Azurite in sibling containers, all on
Docker Desktop on an Apple M4 laptop. Every request is authenticated (JWT validation included). The database starts
with the demo seed and grows during the run (each write iteration declares a claim and moves it through the workflow,
outbox included).

**Traffic mix.** 80 % reads (paginated portfolio list, portfolio statistics with a `GROUP BY`, a claim file with its
history) and 20 % writes (declaration with sequence-allocated number + review transition with `xmin` concurrency check
+ outbox insert).

## Realistic load — 50 concurrent handlers with think time

| | p50 | p95 | p99 |
|---|---|---|---|
| Reads | 1.2 ms | **3.6 ms** | 6.3 ms |
| Writes | 2.4 ms | **5.6 ms** | 13.6 ms |

16 422 requests, **0 errors**, 162 req/s (bounded by the simulated users' think time, not by the API).

## Stress — 100 virtual users, no think time

| | p50 | p95 | p99 |
|---|---|---|---|
| Reads | 14.7 ms | **62 ms** | 95 ms |
| Writes | 28.7 ms | **65 ms** | 93 ms |

**4 344 req/s** sustained over 45 s, 195 754 requests, **0 errors**, 209 MB RAM.

## Reading these numbers

- They come from a laptop with the network in-process (Docker bridge): they show the cost of the application code, not
  of a cloud network hop. In Azure, add the client↔region latency.
- Both runs pass the thresholds encoded in the script (p95 < 200 ms reads, < 400 ms writes, < 1 % errors), so the
  script doubles as a regression gate.
- The outbox poller kept up during the stress run: realtime notifications are not on the request path (ADR 0002).

Reproduce:

```bash
docker build -t claimflow-api .
# start PostgreSQL + Azurite + the API on a shared network (see the commands in this file's history), then:
docker run --rm -i --network <network> -e BASE_URL=http://<api>:8080 grafana/k6 run - < tests/load/claims.js
docker run --rm -i --network <network> -e BASE_URL=http://<api>:8080 -e STRESS=1 grafana/k6 run - < tests/load/claims.js
```
