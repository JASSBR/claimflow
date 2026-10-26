---
name: bind-http-resource
description: "Bind signal-driven read resources in Angular components using httpResource with realtime reloads."
---

> Evidence: value gate verified the facts below against this repository.

Bind signal-driven read resources in Angular components using httpResource with realtime reloads.

Applies in: `claimflow-web`.

## What is specific to this repository

- ClaimsApi only declares command methods, whereas read endpoints are declared directly on components using httpResource. [web/src/app/claims/claims-api.ts:L8-L8]
- CLAIMS_URL is defined as '/api/claims' for claims API requests. [web/src/app/claims/claims-api.ts:L6-L6]
- ClaimDetail defines an ownChange computed signal that returns the change only when realtime.lastChange() matches the component route id. [web/src/app/pages/claim-detail/claim-detail.ts:L65-L68]
- ClaimDetail calls reloadWhen in its constructor to reload the claim resource when ownChange updates. [web/src/app/pages/claim-detail/claim-detail.ts:L84-L86]
- ClaimsList uses reloadWhen in its constructor to reload both claims and stats resources upon realtime.lastChange. [web/src/app/pages/claims-list/claims-list.ts:L73-L75]

## Rules this change touches

- [Invariant] When a decision action is submitted, the claim version must be passed as expectedVersion for optimistic concurrency control; if a conflict occurs, the claim resource must reload to refresh stale data. (approved, conviction 1.00, web/src/app/pages/claim-detail/claim-detail.ts:L93-L119)
- [InformalDecision] ClaimsApi is defined as a root-provided Angular service dedicated strictly to command operations (mutations), leaving read queries to be declared at component rendering sites via httpResource so they remain signal-driven. (approved, conviction 1.00, web/src/app/claims/claims-api.ts:L9-L11)
- [InformalDecision] The Dashboard constructor uses reloadWhen with the realtime.lastChange signal to trigger background reloads of both the stats and recent claims httpResources whenever any claim modification occurs, avoiding skeleton flashes and preserving displayed content while refreshing. (approved, conviction 1.00, web/src/app/pages/dashboard/dashboard.ts:L69-L72)

## Context

- ClaimsList is an Angular component that renders a paginated, filterable, and searchable list of claims, incorporating claim statistics and capabilities, and refreshing its data resources when notified of realtime claim updates. [web/src/app/pages/claims-list/claims-list.ts:L37-L90]
- ClaimDetail.decide submits the requested workflow action along with expectedVersion matching current.version via ClaimsApi.apply; on success it updates the local claim resource, resets the decision panel, and shows a success toast, while on 409 Conflict it reloads the claim to display the latest server version. [web/src/app/pages/claim-detail/claim-detail.ts:L93-L118]
- The ClaimsList component renders the paginated claims list screen, binding status filters from route query parameters via input binding and managing search, status filtering, and pagination signals. [web/src/app/pages/claims-list/claims-list.ts:L41-L50]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/app.routes.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim-labels.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/claims-realtime.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/reload-when.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/toast.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/pages/claim-detail/claim-detail.ts` [web/src/app/pages/claim-detail/claim-detail.ts:L67-L106] is the worked example for this change, as it stands in the repository:

```typescript
    return change?.claimId === this.id() ? change : undefined;
  });

  protected readonly claim = httpResource<ClaimDetails>(() => `${CLAIMS_URL}/${this.id()}`);

  protected readonly documents = httpResource<ClaimDocument[]>(
    () => `${CLAIMS_URL}/${this.id()}/documents`,
    { defaultValue: [] },
  );
  protected readonly analysis = httpResource<ClaimAnalysis | null>(
    () => `${CLAIMS_URL}/${this.id()}/analysis`,
  );
  protected readonly settings = httpResource<DocumentSettings>(() => '/api/documents/settings');
  protected readonly capabilities = httpResource<ClaimCapabilities>(
    () => `${CLAIMS_URL}/capabilities`,
  );

  constructor() {
    reloadWhen(this.ownChange, this.claim);
  }

  protected readonly canWrite = computed(() => this.capabilities.value()?.canDeclare ?? false);
  protected readonly canSettle = computed(
    () => this.capabilities.value()?.roles.includes(ROLES.manager) ?? false,
  );

  protected async decide(decision: DecisionRequest): Promise<void> {
    const current = this.claim.value();
    if (!current) return;

    this.busy.set(true);
    this.errors.set([]);
    try {
      const updated = await firstValueFrom(
        this.api.apply(current.id, { ...decision, expectedVersion: current.version }),
      );
      this.claim.value.set(updated);
      this.decisionPanel()?.reset();
      this.toasts.show({
        tone: 'success',
```

## The same change elsewhere in this repository

- `web/src/app/claims/claims-api.ts` [web/src/app/claims/claims-api.ts:L8-L8]: `/** Commands only. Reads are declared where they are rendered, with httpResource, so they stay signal-driven. */`
- `web/src/app/pages/claims-list/claims-list.ts` [web/src/app/pages/claims-list/claims-list.ts:L58-L58]: `protected readonly claims = httpResource<PagedResponse<ClaimSummary>>(() => {`
- `web/src/app/pages/claims-list/claims-list.ts` [web/src/app/pages/claims-list/claims-list.ts:L74-L74]: `reloadWhen(this.realtime.lastChange, this.claims, this.stats);`
- `web/src/app/pages/dashboard/dashboard.ts` [web/src/app/pages/dashboard/dashboard.ts:L63-L63]: `protected readonly stats = httpResource<ClaimStats>(() => `${CLAIMS_URL}/stats`);`
- `web/src/app/pages/dashboard/dashboard.ts` [web/src/app/pages/dashboard/dashboard.ts:L71-L71]: `reloadWhen(this.realtime.lastChange, this.stats, this.recent);`

## Patterns

Required:
- `ClaimsRealtime`
- `OnPush`
- `httpResource`
- `reloadWhen`
Forbidden:
- Avoid `getClaimById` in this change.
- Avoid `getClaims` in this change.

## Workflow

1. Inject ClaimsRealtime into the component. [web/src/app/pages/claim-detail/claim-detail.ts:L16-L16]
2. Declare the read endpoint with httpResource<T>(() => ...) using CLAIMS_URL and component signals. [web/src/app/pages/claim-detail/claim-detail.ts:L2-L2]
3. If filtering updates for a specific entity ID, define a computed signal comparing realtime.lastChange()?.claimId with the route ID. [web/src/app/pages/claim-detail/claim-detail.ts:L66-L66]
4. Call reloadWhen(...) in the component constructor with the realtime trigger signal and the httpResource instances. [web/src/app/pages/claim-detail/claim-detail.ts:L17-L17]
5. Do not declare read methods in ClaimsApi; declare them directly on the component using httpResource [web/src/app/pages/claim-detail/claim-detail.ts:L21-L21]
6. For entity-specific views, create a computed signal filtering lastChange by claim ID [web/src/app/pages/claim-detail/claim-detail.ts:L66-L66]
7. In the component constructor, invoke reloadWhen passing the change signal and the resources to reload [web/src/app/pages/claim-detail/claim-detail.ts:L17-L17]

## Checklist

- [ ] Declare the resource directly in the component using httpResource, not inside ClaimsApi (reads stay signal-driven). [web/src/app/pages/claim-detail/claim-detail.ts:L21-L21]
- [ ] Provide a URL string or an object with url and query params. [web/src/app/pages/claims-list/claims-list.ts:L41-L41]
- [ ] Pass a defaultValue option if the resource represents a list that should not initially be undefined. [web/src/app/pages/claim-detail/claim-detail.ts:L74-L74]
- [ ] Register reloadWhen in the component constructor. [web/src/app/pages/claim-detail/claim-detail.ts:L17-L17]
- [ ] Use a computed filter on realtime.lastChange when refetching a single entity so unrelated updates are ignored. [web/src/app/pages/claim-detail/claim-detail.ts:L66-L66]
- [ ] Verify httpResource is declared directly in the component, not in ClaimsApi [web/src/app/pages/claim-detail/claim-detail.ts:L21-L21]
- [ ] Ensure ClaimsRealtime is injected and its change signal or a computed slice is passed to reloadWhen [web/src/app/pages/claim-detail/claim-detail.ts:L16-L16]
- [ ] Ensure ChangeDetectionStrategy.OnPush is specified on the component [web/src/app/pages/claim-detail/claim-detail.ts:L4-L4]
- [ ] Verify parameter computation in httpResource returns an object with url and params if query params are dynamic [web/src/app/pages/claim-detail/claim-detail.ts:L2-L2]
