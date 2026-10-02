# Security policy

ClaimFlow is a portfolio project running on fictitious data. If you find a vulnerability anyway, please report it
privately through GitHub's **Report a vulnerability** button (Security tab) rather than in a public issue.

What is in place, and where to look:

| Concern | Implementation |
|---|---|
| Authentication | JWT bearer (OIDC-compatible), options validated at startup — [ADR 0008](docs/adr/0008-authentication-and-segregation-of-duties.md) |
| Authorization | Module-declared policies + domain rules (delegated limits, four-eyes) |
| Input validation | Domain `Result` rules, enums as names only, upload content sniffing, file name sanitising |
| File access | Private blob container; files served only after authorization, scoped by claim id |
| Abuse | Per-IP rate limiting on the API and hub, per-user budget on AI reviews |
| AI | Documents treated as untrusted input (prompt-injection guard), advisory only — [ADR 0009](docs/adr/0009-ai-file-review-with-citations.md) |
| Errors | RFC 9457 problem details, no stack traces or exception messages to clients |
| Headers | `nosniff`, `DENY` framing, `no-referrer`, strict CSP on API responses |
| Supply chain | Central package management, Dependabot, CodeQL, vulnerable-package audit in CI |
| Secrets | None in the repository: user-secrets / Aspire generated parameters locally, Container Apps secrets in Azure |

The demo identity provider issues tokens to anyone by design (see ADR 0008): it is disabled unless `Auth:Mode=Demo`.
