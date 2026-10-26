---
name: add-claim-action
description: "Define a workflow action in the claim domain models with UI metadata, localization, input requirements, and tone configuration."
---

> Evidence: value gate verified the facts below against this repository.

Define a workflow action in the claim domain models with UI metadata, localization, input requirements, and tone configuration.

Applies in: `claimflow-web`.

## What is specific to this repository

- ClaimAction defines the allowed claim action string literals in the claims domain models. [web/src/app/claims/claim.models.ts:L16-L17]
- ActionMeta specifies the metadata required for each action, including label, hint, input flags (needsReason, needsAmount), and tone. [web/src/app/claims/claim-labels.ts:L28-L34]
- DecisionPanel checks if the action requires a reason or amount using ACTION_META flags before prompting or emitting immediately. [web/src/app/pages/claim-detail/decision-panel.ts:L67-L71]

## Rules this change touches

- [Invariant] Allowed workflow action names are 'StartReview', 'RequestInformation', 'ResumeReview', 'Approve', 'Reject', and 'Settle'. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L15-L16)
- [Invariant] The allowedActions on ClaimDetails must represent the intersection of workflow rules, user roles, and four-eyes review policies, pre-filtered for the current user. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L40-L43)
- [Invariant] Claim types consist of the constant array values: 'Auto', 'Home', and 'Liability'. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L12-L13)
- [InformalDecision] The claim models mirror ClaimFlow.Claims.Contracts, with enums serialized as string names via JsonStringEnumConverter and integer representations rejected. (approved, conviction 1.00, web/src/app/claims/claim.models.ts:L1-L1)

## Context

- DecisionRequest defines the payload emitted when a user decides on a claim action, carrying the chosen ClaimAction, an optional reason string, and an optional approvedAmount number. [web/src/app/pages/claim-detail/decision-panel.ts:L15-L19]
- CLAIM_STATUSES defines the exhaustive tuple of six claim lifecycle statuses: Declared, UnderReview, InformationRequested, Approved, Rejected, and Settled, with ClaimStatus typed as their union. [web/src/app/claims/claim.models.ts:L3-L11]
- The confirm method emits a DecisionRequest via the decide output with the currently pending action, a trimmed reason (or undefined if empty), and the approved amount (if the action's metadata requires an amount), returning early without emitting if no action is pending. [web/src/app/pages/claim-detail/decision-panel.ts:L76-L85]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/claims/claims-api.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/claims-realtime.ts` | referenced by or referencing this skill's files |
| `web/src/app/layout/shell.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-declare/claim-declare.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/claim-detail.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/claim-timeline.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/decision-panel.spec.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/workflow-stepper.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/claims/claim.models.ts` [web/src/app/claims/claim.models.ts:L14-L53] is the worked example for this change, as it stands in the repository:

```typescript
export type ClaimType = (typeof CLAIM_TYPES)[number];

export type ClaimAction =
  'StartReview' | 'RequestInformation' | 'ResumeReview' | 'Approve' | 'Reject' | 'Settle';

export interface ClaimSummary {
  readonly id: string;
  readonly number: string;
  readonly policyNumber: string;
  readonly type: ClaimType;
  readonly status: ClaimStatus;
  readonly claimedAmount: number;
  readonly approvedAmount: number | null;
  readonly incidentDate: string;
  readonly declaredAt: string;
  readonly lastUpdatedAt: string;
}

export interface ClaimHistoryEntry {
  readonly from: ClaimStatus;
  readonly to: ClaimStatus;
  readonly action: ClaimAction;
  readonly actorName: string;
  readonly reason: string | null;
  readonly occurredAt: string;
}

export interface ClaimDetails extends ClaimSummary {
  readonly description: string;
  readonly declaredBy: string;
  readonly version: number;
  /** Already filtered for the current user: workflow ∩ role ∩ four-eyes. */
  readonly allowedActions: readonly ClaimAction[];
  readonly history: readonly ClaimHistoryEntry[];
}

export interface PagedResponse<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
```

## The same change elsewhere in this repository

- `web/src/app/claims/claim-labels.ts` [web/src/app/claims/claim-labels.ts:L36-L36]: `export const ACTION_META: Readonly<Record<ClaimAction, ActionMeta>> = {`
- `web/src/app/pages/claim-detail/decision-panel.ts` [web/src/app/pages/claim-detail/decision-panel.ts:L67-L67]: `    const meta = ACTION_META[action];`

## Workflow

1. Add the action name string to ClaimAction in web/src/app/claims/claim.models.ts. [web/src/app/claims/claim.models.ts:L17-L17]
2. Add an entry for the action in ACTION_META within web/src/app/claims/claim-labels.ts. [web/src/app/claims/claim-labels.ts:L36-L36]
3. Configure label, hint, needsReason, needsAmount, and tone properties. [web/src/app/claims/claim-labels.ts:L31-L31]
4. Verify DecisionPanel prompts for reason or amount according to the action metadata flags. [web/src/app/pages/claim-detail/decision-panel.ts:L29-L29]

## Checklist

- [ ] Set needsReason and needsAmount accurately to control whether DecisionPanel prompts for extra inputs. [web/src/app/pages/claim-detail/decision-panel.ts:L29-L29]
