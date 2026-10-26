# Quickstart: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Contracts**: [contracts/reopen-claim.md](./contracts/reopen-claim.md)

Run these commands and scenarios to verify the feature end-to-end against every acceptance criterion in `spec.md`.

## 1. Automated Verification Suite

```bash
# Backend build, unit, architecture, and Testcontainers PostgreSQL integration tests
dotnet build ClaimFlow.slnx
dotnet test --solution ClaimFlow.slnx
dotnet format ClaimFlow.slnx --verify-no-changes

# Frontend i18n extraction/verification, formatting, lint, Vitest unit tests, and FR/EN production build
cd web && npm run i18n && npm run format:check && npm run lint && npm test && npm run build
```

## 2. Scenario Validation Mapping (User Story 1)

1. **Settled claims cannot be reopened (`FR-001`)**:
   - Open the seeded settled claim (`POL-412560`, status `Indemnisé` / `Settled`) as Léa Martin or Karim Benali.
   - Verify `allowedActions` is empty and the decision panel shows `"Dossier clos."` / `"Claim closed."`.
2. **Reopen option available within 30 days (`FR-002`, `FR-003`, `FR-007`, `FR-009`)**:
   - Open the newly seeded rejected claim (rejected ~10 days ago) as Léa Martin (handler) or Karim Benali (manager).
   - Verify the decision panel renders `"Rouvrir le dossier"` (FR) / `"Reopen"` (EN) with hint `"Nouvelles pièces reçues dans les 30 jours suivant le refus."`.
3. **Reopen option removed from day 31 (`FR-003`, `FR-007`)**:
   - Open the seeded rejected claim `POL-509114` (rejected ~90 days ago).
   - Verify `"Reopen"` is not in `allowedActions` and the decision panel shows `"Dossier clos."` / `"Claim closed."`.
   - Verify a direct `POST /api/claims/{id}/actions` with `{"action": "Reopen", ...}` returns `409 Conflict` with code `claim.reopen_window_expired`.
4. **Successful reopening transitions to `UnderReview` (`FR-002`, `FR-008`, `FR-010`)**:
   - On the 10-day rejected claim, click `"Rouvrir le dossier"` / `"Reopen"`, enter reason `"Counter-expertise report uploaded"`, and confirm.
   - Verify the claim transitions to `UnderReview` (`En instruction`) and offers `RequestInformation`, `Approve`, and `Reject`.
5. **Auditor cannot reopen a claim (`FR-006`)**:
   - Log in as Sophie Laurent (`claims.auditor`) and view the 10-day rejected claim.
   - Verify `allowedActions` is empty, the read-only banner is shown, and `POST /api/claims/{id}/actions` returns `403 Forbidden`.
6. **Reopen without a reason is refused (`FR-005`)**:
   - Submit `Reopen` with an empty or whitespace-only reason.
   - Verify `400 Bad Request` with `claim.reason_required` and that the claim remains `Rejected`.
7. **Claim reopened once cannot be reopened again (`FR-004`)**:
   - Reject a previously reopened claim a second time.
   - Verify `allowedActions` is empty (`Reopen` is not offered) and any second `Reopen` request returns `409 Conflict` with `claim.already_reopened`.
8. **Reopening records history and live update (`FR-008`)**:
   - Verify the claim's history table includes the `Rejected → UnderReview` (`Reopen`) entry with actor name, reason, and timestamp, and that a second open browser session receives the `claimChanged` SignalR event and refreshes automatically.
