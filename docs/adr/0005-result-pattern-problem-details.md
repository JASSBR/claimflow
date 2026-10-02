# 0005 — Result pattern + RFC 9457 problem details

- **Status:** Accepted · 2026-10-02
- **Deviates from:** ECC `rules/csharp/patterns.md` ("ApiResponse<T> envelope")

## Context

Business rule violations (invalid policy number, forbidden transition) are **expected** outcomes, not exceptional ones.
Throwing for them makes control flow invisible and costs a stack trace per bad form submission.
On the HTTP side, a custom `{ success, data, error }` envelope duplicates what HTTP status codes already say and is
unknown to every client library.

## Decision

- The domain returns `Result` / `Result<T>` carrying one or more `Error(Code, Description, Type)`.
  `Claim.Declare` returns **all** violated rules at once, so a form shows every problem in one round trip.
- One mapping (`ResultExtensions.ToProblem`) turns errors into **RFC 9457 problem details**:
  `Validation → 400 ValidationProblem` (errors keyed by stable code), `NotFound → 404`, `Conflict → 409` with a `code` extension.
- Successful responses return the resource itself — no envelope.
- Unexpected exceptions hit `UnhandledExceptionHandler`: logged with the trace id, generic 500 to the client, never a stack trace.

## Consequences

- ✅ Clients branch on stable codes (`claim.transition_not_allowed`), never on English messages.
- ✅ Standard `application/problem+json`: Angular, Scalar/OpenAPI and any HTTP tooling understand it.
- ⚠️ Two error paths to know (Result for expected, exceptions for bugs). The rule is written down here on purpose.
