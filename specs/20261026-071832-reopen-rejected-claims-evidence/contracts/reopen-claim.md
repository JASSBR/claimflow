# API & UI Contracts: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

## 1. HTTP Endpoint Contract (`POST /api/claims/{id:guid}/actions`)

### Request (`ClaimActionRequest`)

```json
{
  "action": "Reopen",
  "expectedVersion": 1842,
  "reason": "Counter-expertise report received confirming hail damage occurred after policy inception."
}
```

- **Authorization**: Requires `ClaimPermissions.WritePolicy` (`claims.handler` or `claims.manager`). Callers with `claims.auditor` receive `403 Forbidden`.
- **Optimistic Concurrency**: `expectedVersion` is compared against PostgreSQL `xmin`.

### Responses

- **`200 OK` (`ClaimDetailsResponse`)**:
  - `status`: `"UnderReview"`
  - `allowedActions`: `["RequestInformation", "Approve", "Reject"]` (computed for the caller at `now`)
  - `history`: Includes the appended entry:
    ```json
    {
      "from": "Rejected",
      "to": "UnderReview",
      "action": "Reopen",
      "actorName": "Léa Martin",
      "reason": "Counter-expertise report received confirming hail damage occurred after policy inception.",
      "occurredAt": "2026-10-26T10:00:00+00:00"
    }
    ```
- **`400 Bad Request` (`application/problem+json` — ValidationProblemDetails)**:
  - Returned when `reason` is missing or whitespace (`errors["claim.reason_required"]`).
- **`403 Forbidden` (`application/problem+json` — ProblemDetails)**:
  - Returned when the caller is in the `claims.auditor` role.
- **`409 Conflict` (`application/problem+json` — ProblemDetails)**:
  - `code: "claim.reopen_window_expired"` — More than 30 days have elapsed since the rejection timestamp.
  - `code: "claim.already_reopened"` — The claim was already reopened once before.
  - `code: "claim.transition_not_allowed"` — The claim is not in `Rejected` status (e.g., `Settled`, `Declared`, `UnderReview`, `Approved`).
  - `code: "claim.concurrent_update"` — `expectedVersion` does not match current `xmin`.

## 2. Read Contract (`GET /api/claims/{id:guid}`)

- Computes `allowedActions` via `ClaimPermissions.AllowedActions(claim, user, timeProvider.GetUtcNow())`:
  - Includes `"Reopen"` when `claim.Status == Rejected`, the caller is a handler or manager, `claim.History` has no prior `Reopen` entry, and `now - lastReject.OccurredAt <= TimeSpan.FromDays(30)`.
  - Omits `"Reopen"` when `now - lastReject.OccurredAt > TimeSpan.FromDays(30)`, when `claim.History` already contains a `Reopen` entry, or when the caller is an auditor.

## 3. Realtime SignalR Contract (`/hubs/claims`, event `claimChanged`)

- Dispatched asynchronously from `claims.outbox_messages` via `WhenClaimChangedNotifyHub`:
  ```json
  {
    "claimId": "...",
    "number": "SIN-2026-000007",
    "status": "UnderReview",
    "actorName": "Léa Martin",
    "occurredAt": "2026-10-26T10:00:00+00:00"
  }
  ```

## 4. Angular Frontend Contract (`web/src/app/claims/`)

- **`ClaimAction` (`claim.models.ts`)**:
  ```ts
  export type ClaimAction =
    | 'StartReview'
    | 'RequestInformation'
    | 'ResumeReview'
    | 'Approve'
    | 'Reject'
    | 'Reopen'
    | 'Settle';
  ```
- **`ACTION_META.Reopen` (`claim-labels.ts`)**:
  - French label (`@@action.reopen`): `"Rouvrir le dossier"`
  - English label (`@@action.reopen`): `"Reopen"`
  - French hint (`@@action.reopen.hint`): `"Nouvelles pièces reçues dans les 30 jours suivant le refus."`
  - English hint (`@@action.reopen.hint`): `"New evidence received within 30 days of rejection."`
  - `needsReason`: `true`
  - `needsAmount`: `false`
  - `tone`: `'primary'`
- **Error Code Localization (`shared/problem-details.ts` & `translations.en.json`)**:
  - `claim.reopen_window_expired` (`@@error.reopenWindowExpired`):
    - FR: `"Ce dossier ne peut plus être rouvert : le délai de 30 jours après le refus est dépassé."`
    - EN: `"This claim can no longer be reopened: the 30-day window after rejection has expired."`
  - `claim.already_reopened` (`@@error.alreadyReopened`):
    - FR: `"Ce dossier a déjà été rouvert une fois et ne peut pas l'être à nouveau."`
    - EN: `"This claim has already been reopened once and cannot be reopened again."`
