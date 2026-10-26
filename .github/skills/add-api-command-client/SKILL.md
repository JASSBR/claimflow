---
name: add-api-command-client
description: "Add a mutation command to ClaimsApi, invoke it from a component with error handling, and add an injectable API client service for HTTP commands."
---

> Evidence: value gate verified the facts below against this repository.

Add a mutation command to ClaimsApi, invoke it from a component with error handling, and add an injectable API client service for HTTP commands.

Applies in: `claimflow-web`.

## What is specific to this repository

- API mutation errors are converted into displayable error messages using problemMessages(error). [web/src/app/pages/claim-declare/claim-declare.ts:L101-L101]
- When a decision submission fails due to a conflict error checked with isConflict(error), the claim resource is reloaded. [web/src/app/pages/claim-detail/claim-detail.ts:L112-L115]
- Claim decisions are applied via ClaimsApi with the current claim ID and expectedVersion to track concurrency. [web/src/app/pages/claim-detail/claim-detail.ts:L101-L101]

## Rules this change touches

- [Invariant] When a decision action is submitted, the claim version must be passed as expectedVersion for optimistic concurrency control; if a conflict occurs, the claim resource must reload to refresh stale data. (approved, conviction 1.00, web/src/app/pages/claim-detail/claim-detail.ts:L93-L119)

## Context

- ClaimDetail.decide submits the requested workflow action along with expectedVersion matching current.version via ClaimsApi.apply; on success it updates the local claim resource, resets the decision panel, and shows a success toast, while on 409 Conflict it reloads the claim to display the latest server version. [web/src/app/pages/claim-detail/claim-detail.ts:L93-L118]
- onSubmit handles claim declaration form submission: it clears server errors, executes submit on the signal form, calls ClaimsApi.declare with the form model, shows a success toast and redirects to /claims/:id on success, or sets serverErrors from API problem details on failure. [web/src/app/pages/claim-declare/claim-declare.ts:L89-L104]
- `ClaimDetail` is an Angular component representing the claim details page. It reads the route parameter `id` via component input binding, coordinates HTTP resources for claim details, documents, analysis, document settings, and capabilities, and subscribes to realtime claim change events via `reloadWhen`. [web/src/app/pages/claim-detail/claim-detail.ts:L50-L83]

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

`web/src/app/pages/claim-declare/claim-declare.ts` [web/src/app/pages/claim-declare/claim-declare.ts:L91-L106] is the worked example for this change, as it stands in the repository:

```typescript
    this.serverErrors.set([]);
    await submit(this.form, async () => {
      try {
        const claim = await firstValueFrom(this.api.declare(this.model()));
        this.toasts.show({
          tone: 'success',
          title: $localize`:@@toast.declared:Sinistre ${claim.number}:number: déclaré`,
        });
        await this.router.navigate(['/claims', claim.id]);
      } catch (error) {
        this.serverErrors.set(problemMessages(error));
      }
      return undefined;
    });
  }
}
```

## The same change elsewhere in this repository

- `web/src/app/pages/claim-detail/claim-detail.ts` [web/src/app/pages/claim-detail/claim-detail.ts:L100-L100]: `      const updated = await firstValueFrom(`

## Workflow

1. Inject ClaimsApi and ToastService into the invoking component. [web/src/app/pages/claim-declare/claim-declare.ts:L18-L18]
2. Invoke the API method inside an async handler using await firstValueFrom(...). [web/src/app/pages/claim-declare/claim-declare.ts:L14-L14]
3. Notify on success with this.toasts.show({ tone: 'success', ... }). [web/src/app/pages/claim-declare/claim-declare.ts:L95-L95]
4. Catch errors and extract error messages using problemMessages(error). [web/src/app/pages/claim-declare/claim-declare.ts:L20-L20]

## Checklist

- [ ] Only add write/mutation methods to ClaimsApi; do not add read endpoints. [web/src/app/pages/claim-declare/claim-declare.ts:L18-L18]
- [ ] Ensure expectedVersion is included in mutating action payloads for optimistic concurrency. [web/src/app/pages/claim-detail/claim-detail.ts:L101-L101]
- [ ] Convert the Observable to a Promise in async handlers using firstValueFrom. [web/src/app/pages/claim-declare/claim-declare.ts:L14-L14]
- [ ] Parse error payloads using problemMessages(error) to populate component error signals. [web/src/app/pages/claim-declare/claim-declare.ts:L20-L20]
- [ ] Show feedback upon success using ToastService.show with localized title. [web/src/app/pages/claim-declare/claim-declare.ts:L15-L15]
- [ ] Do not place standard query reads in this service; leave reads to component-level httpResource. [web/src/app/pages/claim-detail/claim-detail.ts:L2-L2]
