---
name: add-claim-status
description: "Register a new claim status across domain models, localization dictionaries, workflow steppers, and dashboard color maps."
---

> Evidence: value gate verified the facts below against this repository.

Register a new claim status across domain models, localization dictionaries, workflow steppers, and dashboard color maps.

Applies in: `claimflow-web`.

## What is specific to this repository

- Claim statuses are defined as a const array of string literals and typed as a union of its members. [web/src/app/claims/claim.models.ts:L3-L11]
- Localized claim status labels and activity feed verbs are mapped by ClaimStatus using $localize tokens. [web/src/app/claims/claim-labels.ts:L3-L10]
- The happy path status progression for the stepper is configured in WORKFLOW_STEPS. [web/src/app/claims/claim-labels.ts:L81-L87]
- StatusBadge styles assign CSS custom properties for color based on [data-status] attribute selectors. [web/src/app/shared/status-badge.ts:L29-L31]

## Rules this change touches

- [Invariant] Allowed workflow action names are 'StartReview', 'RequestInformation', 'ResumeReview', 'Approve', 'Reject', and 'Settle'. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L15-L16)
- [Invariant] The allowedActions on ClaimDetails must represent the intersection of workflow rules, user roles, and four-eyes review policies, pre-filtered for the current user. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L40-L43)
- [Invariant] Claim types consist of the constant array values: 'Auto', 'Home', and 'Liability'. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L12-L13)
- [InformalDecision] The claim models mirror ClaimFlow.Claims.Contracts, with enums serialized as string names via JsonStringEnumConverter and integer representations rejected. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L1-L1)
- [InformalDecision] The Dashboard constructor uses reloadWhen with the realtime.lastChange signal to trigger background reloads of both the stats and recent claims httpResources whenever any claim modification occurs, avoiding skeleton flashes and preserving displayed content while refreshing. (approved, conviction 1.00, web/src/app/pages/dashboard/dashboard.ts:L69-L72)

## Context

- CLAIM_STATUSES defines the exhaustive tuple of six claim lifecycle statuses: Declared, UnderReview, InformationRequested, Approved, Rejected, and Settled, with ClaimStatus typed as their union. [web/src/app/claims/claim.models.ts:L3-L11]
- The Dashboard component displays the claims management dashboard, fetching claims statistics and recent claims via httpResource, tracking realtime claim changes to trigger reloads, and exposing computed signals for user greetings, key performance indicators (KPIs), claim status breakdown slices, and claims distribution by type. [web/src/app/pages/dashboard/dashboard.ts:L51-L110]
- STATUS_LABELS maps every ClaimStatus ('Declared', 'UnderReview', 'InformationRequested', 'Approved', 'Rejected', 'Settled') to its localized French display label ('Déclaré', 'En instruction', 'Pièces demandées', 'Accepté', 'Refusé', 'Indemnisé') using Angular's $localize tag. [web/src/app/claims/claim-labels.ts:L3-L10]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/app.routes.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claims-api.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/claims-realtime.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/reload-when.ts` | referenced by or referencing this skill's files |
| `web/src/app/layout/shell.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-declare/claim-declare.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/claim-detail.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/shared/status-badge.ts` [web/src/app/shared/status-badge.ts:L48-L53] is the worked example for this change, as it stands in the repository:

```typescript
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadge {
  readonly status = input.required<ClaimStatus>();
  protected readonly label = computed(() => STATUS_LABELS[this.status()]);
}
```

## The same change elsewhere in this repository

- `web/src/app/claims/claim.models.ts` [web/src/app/claims/claim.models.ts:L3-L3]: `export const CLAIM_STATUSES = [`
- `web/src/app/claims/claim-labels.ts` [web/src/app/claims/claim-labels.ts:L3-L3]: `export const STATUS_LABELS: Readonly<Record<ClaimStatus, string>> = {`
- `web/src/app/claims/claim-labels.ts` [web/src/app/claims/claim-labels.ts:L13-L13]: `export const STATUS_VERBS: Readonly<Record<ClaimStatus, string>> = {`
- `web/src/app/claims/claim-labels.ts` [web/src/app/claims/claim-labels.ts:L82-L82]: `export const WORKFLOW_STEPS: readonly ClaimStatus[] = [`
- `web/src/app/pages/dashboard/dashboard.ts` [web/src/app/pages/dashboard/dashboard.ts:L24-L24]: `const STATUS_COLORS: Record<string, string> = {`

## Patterns

Required:
- `$localize`
- `CLAIM_STATUSES`
- `STATUS_LABELS`
- `STATUS_VERBS`

## Workflow

1. Add the status string literal to CLAIM_STATUSES in claim.models.ts [web/src/app/claims/claim.models.ts:L3-L3]
2. Add localized entries to STATUS_LABELS and STATUS_VERBS in claim-labels.ts using $localize [web/src/app/shared/status-badge.ts:L2-L2]
3. If the status is part of the standard happy path progression, update WORKFLOW_STEPS in claim-labels.ts [web/src/app/claims/claim-labels.ts:L82-L82]
4. Add a [data-status] CSS selector block in StatusBadge styles in status-badge.ts [web/src/app/shared/status-badge.ts:L50-L50]

## Checklist

- [ ] Ensure the new status name is added to the CLAIM_STATUSES const array in claim.models.ts [web/src/app/claims/claim.models.ts:L3-L3]
- [ ] Verify localized strings are added to STATUS_LABELS and STATUS_VERBS using $localize [web/src/app/shared/status-badge.ts:L2-L2]
- [ ] Add CSS variable mapping in StatusBadge styles ([data-status=...]) [web/src/app/shared/status-badge.ts:L50-L50]
- [ ] Map the status to a CSS color variable in STATUS_COLORS in dashboard.ts [web/src/app/pages/dashboard/dashboard.ts:L24-L24]
- [ ] If part of standard progress, append the status to WORKFLOW_STEPS in claim-labels.ts [web/src/app/claims/claim-labels.ts:L82-L82]
