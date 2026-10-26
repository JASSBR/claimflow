---
name: add-routed-page
description: "Register lazy-loaded page routes under the portal router with authentication guards."
---

> Evidence: value gate verified the facts below against this repository.

Register lazy-loaded page routes under the portal router with authentication guards.

Applies in: `claimflow-web`.

## What is specific to this repository

- reloadWhen refreshes reloadable resources when a trigger changes while keeping existing on-screen data loaded to prevent view flickering. [web/src/app/core/realtime/reload-when.ts:L8-L8]
- Route parameters are mapped to component inputs using withComponentInputBinding(). [web/src/app/pages/claim-detail/claim-detail.ts:L56-L56]
- API error responses are parsed into user-facing messages using the problemMessages helper. [web/src/app/pages/claim-declare/claim-declare.ts:L101-L101]

## Rules this change touches

- [Invariant] reloadWhen suppresses reloading on the initial run of its effect, only triggering resource.reload() calls on subsequent executions caused by reactive trigger changes. (approved, conviction 1.00, web/src/app/core/realtime/reload-when.ts:L14-L19)
- [Invariant] When a decision action is submitted, the claim version must be passed as expectedVersion for optimistic concurrency control; if a conflict occurs, the claim resource must reload to refresh stale data. (approved, conviction 1.00, web/src/app/pages/claim-detail/claim-detail.ts:L93-L119)
- [Assumption] reloadWhen assumes it is called within an Angular injection context, such as in a component field initializer. (approved, conviction 1.00, web/src/app/core/realtime/reload-when.ts:L11-L13)
- [InformalDecision] reloadWhen triggers resource.reload() rather than mutating resource request parameters so that current screen values are retained during loading, avoiding skeleton flashes and user state loss. (approved, conviction 1.00, web/src/app/core/realtime/reload-when.ts:L7-L10)
- [InformalDecision] Resource reload calls inside reloadWhen are wrapped in untracked() to ensure any reactive signals read during reload() do not become tracking dependencies of the trigger effect. (approved, conviction 1.00, web/src/app/core/realtime/reload-when.ts:L20-L20)
- [InformalDecision] The Dashboard constructor uses reloadWhen with the realtime.lastChange signal to trigger background reloads of both the stats and recent claims httpResources whenever any claim modification occurs, avoiding skeleton flashes and preserving displayed content while refreshing. (approved, conviction 1.00, web/src/app/pages/dashboard/dashboard.ts:L69-L72)
- [InformalDecision] The application route table lazily loads all top-level and nested component views (Login, Shell, Dashboard, ClaimsList, ClaimDeclare, and ClaimDetail) via dynamic ESM import() expressions. (approved, conviction 1.00, web/src/app/app.routes.ts:L4-L38)

## Context

- ClaimsList is an Angular component that renders a paginated, filterable, and searchable list of claims, incorporating claim statistics and capabilities, and refreshing its data resources when notified of realtime claim updates. [web/src/app/pages/claims-list/claims-list.ts:L37-L90]
- ClaimDetail.decide submits the requested workflow action along with expectedVersion matching current.version via ClaimsApi.apply; on success it updates the local claim resource, resets the decision panel, and shows a success toast, while on 409 Conflict it reloads the claim to display the latest server version. [web/src/app/pages/claim-detail/claim-detail.ts:L93-L118]
- onSubmit handles claim declaration form submission: it clears server errors, executes submit on the signal form, calls ClaimsApi.declare with the form model, shows a success toast and redirects to /claims/:id on success, or sets serverErrors from API problem details on failure. [web/src/app/pages/claim-declare/claim-declare.ts:L89-L104]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/app.config.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim-labels.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claims-api.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.guard.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/claims-realtime.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/pages/claim-declare/claim-declare.ts` [web/src/app/pages/claim-declare/claim-declare.ts:L36-L75] is the worked example for this change, as it stands in the repository:

```typescript
  styleUrl: './claim-declare.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimDeclare {
  private readonly api = inject(ClaimsApi);
  private readonly router = inject(Router);
  private readonly toasts = inject(ToastService);

  protected readonly types = CLAIM_TYPES;
  protected readonly typeLabels = TYPE_LABELS;
  protected readonly serverErrors = signal<string[]>([]);

  protected readonly model = signal<DeclareModel>({
    policyNumber: '',
    type: 'Auto',
    incidentDate: today(),
    description: '',
    claimedAmount: 0,
  });

  // Client rules mirror the domain for instant feedback; the API stays the authority (its errors are shown too).
  protected readonly form = form(this.model, (path) => {
    required(path.policyNumber, {
      message: $localize`:@@declare.policyRequired:Le numéro de contrat est obligatoire.`,
    });
    pattern(path.policyNumber, /^POL-\d{6}$/i, {
      message: $localize`:@@declare.policyFormat:Format attendu : POL-000000.`,
    });
    required(path.incidentDate, {
      message: $localize`:@@declare.dateRequired:La date de survenance est obligatoire.`,
    });
    validate(path.incidentDate, ({ value }) =>
      value() > today()
        ? {
            kind: 'future',
            message: $localize`:@@declare.dateFuture:La date ne peut pas être dans le futur.`,
          }
        : null,
    );
    required(path.description, {
```

## The same change elsewhere in this repository

- `web/src/app/core/realtime/reload-when.ts` [web/src/app/core/realtime/reload-when.ts:L13-L13]: `export function reloadWhen(trigger: () => unknown, ...resources: Reloadable[]): void {`
- `web/src/app/pages/claim-detail/claim-detail.ts` [web/src/app/pages/claim-detail/claim-detail.ts:L50-L50]: `export class ClaimDetail {`
- `web/src/app/pages/claim-detail/claim-detail.ts` [web/src/app/pages/claim-detail/claim-detail.ts:L57-L57]: `  readonly id = input.required<string>();`
- `web/src/app/pages/claims-list/claims-list.ts` [web/src/app/pages/claims-list/claims-list.ts:L37-L37]: `export class ClaimsList {`
- `web/src/app/pages/dashboard/dashboard.ts` [web/src/app/pages/dashboard/dashboard.ts:L51-L51]: `export class Dashboard {`
- `web/src/app/app.routes.ts` [web/src/app/app.routes.ts:L4-L4]: `export const routes: Routes = [`
- `web/src/app/app.routes.ts` [web/src/app/app.routes.ts:L8-L8]: `canActivate: [guestGuard],`
- `web/src/app/app.routes.ts` [web/src/app/app.routes.ts:L13-L13]: `canActivate: [authGuard],`
- `web/src/app/app.routes.ts` [web/src/app/app.routes.ts:L14-L14]: `loadComponent: () => import('./layout/shell').then((m) => m.Shell),`
- `web/src/app/app.routes.ts` [web/src/app/app.routes.ts:L18-L18]: `path: 'dashboard',`
- `web/src/app/app.routes.ts` [web/src/app/app.routes.ts:L19-L19]: `title: 'Tableau de bord · ClaimFlow',`

## Patterns

Required:
- `ChangeDetectionStrategy.OnPush`
- `canActivate:`
- `httpResource`
- `import(`
- `loadComponent`
- `loadComponent:`
- `reloadWhen`
- `title:`
Forbidden:
- Avoid `NgModule` in this change.

## Workflow

1. Create the page component in web/src/app/pages/<name>/ with OnPush change detection and companion HTML/SCSS files. [web/src/app/pages/claim-declare/claim-declare.ts:L39-L39]
2. Declare route path and query parameters as signals using input.required() or input(). [web/src/app/pages/claim-detail/claim-detail.ts:L57-L57]
3. Declare read data sources using httpResource<T>() with reactive URL computeds. [web/src/app/pages/claim-detail/claim-detail.ts:L2-L2]
4. Inject ClaimsRealtime and call reloadWhen(() => this.realtime.lastChange(), ...resources) to refresh data without view flicker. [web/src/app/pages/claim-detail/claim-detail.ts:L16-L16]
5. Handle user mutations by injecting command services like ClaimsApi and mapping errors with problemMessages. [web/src/app/pages/claim-declare/claim-declare.ts:L18-L18]

## Checklist

- [ ] Does the route specify title ending with ' · ClaimFlow'? [web/src/app/app.routes.ts:L7-L7]
- [ ] Is the route registered as a child under the Shell layout if it requires authentication? [web/src/app/app.routes.ts:L14-L14]
- [ ] Is the route lazy-loaded with loadComponent using import().then(m => m.Component)? [web/src/app/app.routes.ts:L9-L9]
- [ ] Is unauthenticated routing protected with guestGuard at the root level? [web/src/app/app.routes.ts:L2-L2]
- [ ] The route path is configured either under guestGuard or inside the Shell authenticated children protected by authGuard. [web/src/app/app.routes.ts:L2-L2]
- [ ] Component loading is lazy via loadComponent with dynamic import(). [web/src/app/app.routes.ts:L9-L9]
- [ ] The loaded component is configured with ChangeDetectionStrategy.OnPush. [web/src/app/pages/claim-declare/claim-declare.ts:L1-L1]
- [ ] Declare route parameters as signal inputs with input.required() or input(). [web/src/app/pages/claim-detail/claim-detail.ts:L57-L57]
- [ ] Declare read queries with httpResource rather than imperative HttpClient calls. [web/src/app/pages/claim-detail/claim-detail.ts:L2-L2]
- [ ] Connect reloadWhen to this.realtime.lastChange() in the injection context to reload resources seamlessly. [web/src/app/pages/claim-detail/claim-detail.ts:L66-L66]
- [ ] Register the new route in web/src/app/app.routes.ts with lazy loadComponent. [web/src/app/app.routes.ts:L4-L4]
- [ ] Set changeDetection to ChangeDetectionStrategy.OnPush on the page component. [web/src/app/pages/claim-declare/claim-declare.ts:L1-L1]
