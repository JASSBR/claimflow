---
name: add-detail-panel
description: "Add a presentational or action-oriented subpanel component for the claim detail view."
---

> Evidence: value gate verified the facts below against this repository.

Add a presentational or action-oriented subpanel component for the claim detail view.

Applies in: `claimflow-web`.

## What is specific to this repository

- Claim detail subpanel components such as AnalysisPanel, ClaimTimeline, DecisionPanel, DocumentsPanel, and WorkflowStepper are located locally and imported into ClaimDetail. [web/src/app/pages/claim-detail/claim-detail.ts:L26-L30]
- DecisionPanel defines inputs for ClaimDetails and capabilities, and outputs a DecisionRequest via output(). [web/src/app/pages/claim-detail/decision-panel.ts:L30-L38]
- Panel components reference status labels and workflow step definitions from claim-labels. [web/src/app/pages/claim-detail/workflow-stepper.ts:L2-L2]

## Rules this change touches

- [Invariant] When a decision action is submitted, the claim version must be passed as expectedVersion for optimistic concurrency control; if a conflict occurs, the claim resource must reload to refresh stale data. (approved, conviction 1.00, web/src/app/pages/claim-detail/claim-detail.ts:L93-L119)
- [Invariant] Step markers display a check icon ('check') when step state is 'done', an x icon ('x') when step state is 'rejected', and the 1-based index number ($index + 1) for all other states. (approved, conviction 1.00, web/src/app/pages/claim-detail/workflow-stepper.ts:L18-L29)
- [BusinessRule] When the claim status is 'InformationRequested', it maps to index 1 of the workflow steps (replacing the 'UnderReview' label with 'InformationRequested' label) and sets that second step to 'current'. (approved, conviction 1.00, web/src/app/pages/claim-detail/workflow-stepper.ts:L104-L136)
- [BusinessRule] When a claim's status is 'Rejected', the workflow steps are truncated to a fixed 3-step sequence: 'Declared' (done), 'UnderReview' (done), and 'Rejected' (rejected), discarding subsequent happy path steps. (approved, conviction 1.00, web/src/app/pages/claim-detail/workflow-stepper.ts:L106-L123)
- [BusinessRule] For non-rejected claims, each step in WORKFLOW_STEPS is marked 'done' if its index is prior to the active position or if the claim is 'Settled', 'current' if its index matches the position, and 'todo' otherwise. (approved, conviction 1.00, web/src/app/pages/claim-detail/workflow-stepper.ts:L124-L136)

## Context

- The openSource method opens the cited document via DocumentsApi.open, passing the claim ID, the citation's documentId, and its startPage. [web/src/app/pages/claim-detail/analysis-panel.ts:L70-L72]
- DecisionRequest defines the payload emitted when a user decides on a claim action, carrying the chosen ClaimAction, an optional reason string, and an optional approvedAmount number. [web/src/app/pages/claim-detail/decision-panel.ts:L15-L19]
- The ngOnDestroy lifecycle hook clears the active ticker interval to prevent background timer leaks when the component is unmounted. [web/src/app/pages/claim-detail/analysis-panel.ts:L94-L96]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/app.routes.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim-labels.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claims-api.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/claims-realtime.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/reload-when.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/pages/claim-detail/claim-timeline.ts` [web/src/app/pages/claim-detail/claim-timeline.ts:L87-L94] is the worked example for this change, as it stands in the repository:

```typescript
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimTimeline {
  readonly claim = input.required<ClaimDetails>();
  protected readonly actions = ACTION_META;
  protected readonly statuses = STATUS_LABELS;
}
```

## The same change elsewhere in this repository

- `web/src/app/pages/claim-detail/workflow-stepper.ts` [web/src/app/pages/claim-detail/workflow-stepper.ts:L113-L113]: `export class WorkflowStepper {`
- `web/src/app/pages/claim-detail/claim-detail.ts` [web/src/app/pages/claim-detail/claim-detail.ts:L50-L50]: `export class ClaimDetail {`
- `web/src/app/pages/claim-detail/decision-panel.ts` [web/src/app/pages/claim-detail/decision-panel.ts:L29-L29]: `export class DecisionPanel {`
- `web/src/app/pages/claim-detail/analysis-panel.ts` [web/src/app/pages/claim-detail/analysis-panel.ts:L37-L37]: `export class AnalysisPanel implements OnDestroy {`
- `web/src/app/pages/claim-detail/documents-panel.ts` [web/src/app/pages/claim-detail/documents-panel.ts:L17-L17]: `export class DocumentsPanel {`

## Patterns

Required:
- `ChangeDetectionStrategy.OnPush`
- `input`
Forbidden:
- Avoid `@Input()` in this change.
- Avoid `@Output()` in this change.

## Workflow

1. Create the panel component in web/src/app/pages/claim-detail/<panel-name>.ts. [web/src/app/pages/claim-detail/workflow-stepper.ts:L113-L113]
2. Configure @Component with OnPush change detection and imported directives/pipes. [web/src/app/pages/claim-detail/claim-timeline.ts:L88-L88]
3. Define data and capability inputs using input() or input.required(). [web/src/app/pages/claim-detail/claim-timeline.ts:L91-L91]
4. Import the panel into ClaimDetail and render it within claim-detail.html. [web/src/app/pages/claim-detail/claim-detail.ts:L50-L50]

## Checklist

- [ ] Set ChangeDetectionStrategy.OnPush on the panel component. [web/src/app/pages/claim-detail/claim-timeline.ts:L2-L2]
- [ ] Declare inputs using input() or input.required(). [web/src/app/pages/claim-detail/claim-timeline.ts:L91-L91]
- [ ] Look up labels and action metadata from claim-labels.ts (e.g. ACTION_META, STATUS_LABELS). [web/src/app/pages/claim-detail/claim-timeline.ts:L3-L3]
- [ ] Import and place the panel component in ClaimDetail. [web/src/app/pages/claim-detail/claim-detail.ts:L50-L50]
