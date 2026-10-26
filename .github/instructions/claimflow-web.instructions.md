---
applyTo: 'web/**'
---

# claimflow-web

`web/` is a npm project of this repository, declared by `web/package.json`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `npm --prefix web run build`
- `npm --prefix web run lint`
- `npm --prefix web run test`

## Rules

Each rule cites the code it rests on.

- [Invariant] The rendered stroke length of an SVG donut arc slice is always at least 0.01 units. [web/src/app/shared/charts/donut-chart.ts:L93-L93]
- [Invariant] reloadWhen suppresses reloading on the initial run of its effect, only triggering resource.reload() calls on subsequent executions caused by reactive trigger changes. [web/src/app/core/realtime/reload-when.ts:L14-L19]
- [Invariant] When a decision action is submitted, the claim version must be passed as expectedVersion for optimistic concurrency control; if a conflict occurs, the claim resource must reload to refresh stale data. [web/src/app/pages/claim-detail/claim-detail.ts:L93-L119]
- [Invariant] Allowed workflow action names are 'StartReview', 'RequestInformation', 'ResumeReview', 'Approve', 'Reject', and 'Settle'. [web/src/app/claims/claim.models.ts:L15-L16]
- [Invariant] The allowedActions on ClaimDetails must represent the intersection of workflow rules, user roles, and four-eyes review policies, pre-filtered for the current user. [web/src/app/claims/claim.models.ts:L40-L43]
- [Invariant] The divisor max in the percentage calculation is always at least 1, preventing division by zero even if all item values are 0 or empty. [web/src/app/shared/charts/bar-list.ts:L53-L53]
- [Invariant] When LOCALE_ID starts with 'en', the target switch destination is 'fr'; otherwise, it is 'en'. The resulting href always prepends the target locale prefix to the current pathname stripped of any leading '/en' or '/fr'. [web/src/app/core/locale.ts:L14-L21]
- [Invariant] Toggling the theme switches the current theme from `'dark'` to `'light'`, or from any other value (such as `'light'`) to `'dark'`. [web/src/app/core/theme.ts:L25-L27]
- [Invariant] Authentication credentials must never be attached to requests targeting '/api/auth/' authentication endpoints (such as token retrieval) or external/non-API URLs. [web/src/app/core/auth/auth.interceptor.ts:L10-L10]
- [Invariant] While an authentication request for a persona is in flight, the pending signal stores that persona's identifier and is cleared only if the login operation encounters an error. [web/src/app/pages/login/login.ts:L78-L88]
- [Invariant] Step markers display a check icon ('check') when step state is 'done', an x icon ('x') when step state is 'rejected', and the 1-based index number ($index + 1) for all other states. [web/src/app/pages/claim-detail/workflow-stepper.ts:L18-L29]
- [Invariant] The rendered SVG element always marks aria-hidden="true", renders with stroke currentColor and stroke-width 1.9, and sizes both width and height to size(). [web/src/app/shared/icon.ts:L32-L47]
- [Invariant] Claim types consist of the constant array values: 'Auto', 'Home', and 'Liability'. [web/src/app/claims/claim.models.ts:L12-L13]
- [Invariant] Initial theme resolution priority first checks `localStorage` key `'claimflow.theme'` for a valid `'light'` or `'dark'` value; if absent or inaccessible, it defaults to the system's `prefers-color-scheme: dark` media query if matched, otherwise falling back to `'light'`. [web/src/app/core/theme.ts:L30-L38]
- [Invariant] Upon successful submission of a valid claim declaration to /api/claims, navigation is triggered to the newly created claim details page /claims/:id [web/src/app/pages/claim-declare/claim-declare.spec.ts:L57-L72]
- [Invariant] reloadWhen does not invoke reload() on resource creation or initial tick, but invokes reload() on each subsequent change of the trigger signal [web/src/app/core/realtime/reload-when.spec.ts:L6-L26]
- [Invariant] When a claim decision encounters an HTTP 409 Conflict, the conflict error message is displayed in an alert and the claim details are reloaded from the API. [web/src/app/pages/claim-detail/claim-detail.spec.ts:L65-L80]
- [Invariant] When a claim's indemnity is released, the claim status updates in real time to 'Indemnisé' across open sessions viewing that claim. [web/e2e/four-eyes-realtime.spec.ts:L24-L27]
- [Invariant] The seeded collision claim associated with policy 'POL-104233' contains exactly three documents in its documents panel. [web/e2e/documents-and-roles.spec.ts:L14-L20]
- [Invariant] Applying a search query resets the pagination to page 1 regardless of the current page. [web/src/app/pages/claims-list/claims-list.spec.ts:L90-L102]
- [Invariant] ClaimsRealtime updates lastChange to the most recent notification and prepends incoming notifications to the activity feed (newest first). [web/src/app/core/realtime/claims-realtime.spec.ts:L56-L62]
- [BusinessRule] The action to declare a new claim ('/claims/new') is only presented to users whose capabilities include 'canDeclare'. [web/src/app/pages/claims-list/claims-list.html:L10-L14]
- [BusinessRule] Running or re-running the AI claim analysis requires at least one document attached to the claim; if documentCount() is 0, the action button is disabled and an informational notice prompts the user to add at least one document. [web/src/app/pages/claim-detail/analysis-panel.html:L90-L106]
- [BusinessRule] When mapping a persona's assigned roles to a localized display label, if the persona includes the 'claims.manager' role, the label is 'Responsable'; otherwise if it includes 'claims.auditor', the label is 'Lecture seule'; otherwise the default label is 'Gestionnaire'. [web/src/app/pages/login/login.ts:L72-L76]

## Skills

- `add-claim-detail-panel`: Add a claim detail facet panel component in the Angular web client to inspect or process claim attachments and analysis. (`.github/skills/add-claim-detail-panel/SKILL.md`)
- `add-routed-page`: Register lazy-loaded page routes under the portal router with authentication guards. (`.github/skills/add-routed-page/SKILL.md`)
- `add-portal-icon`: Add inline SVG stroke icons to the portal icon dictionary on the 24x24 grid. (`.github/skills/add-portal-icon/SKILL.md`)
- `map-api-error`: Map a backend RFC 9457 domain error code to a localized user-facing message in the web portal error registry. (`.github/skills/map-api-error/SKILL.md`)
- `add-localized-ui-string`: Add user-facing text to Angular templates with explicit @@ message IDs and provide corresponding English translations in the translation catalog. (`.github/skills/add-localized-ui-string/SKILL.md`)
- `bind-http-resource`: Bind signal-driven read resources in Angular components using httpResource with realtime reloads. (`.github/skills/bind-http-resource/SKILL.md`)
- `add-chart-component`: Add a dependency-free SVG or CSS visualization chart component in shared/charts with signal inputs and OnPush change detection. (`.github/skills/add-chart-component/SKILL.md`)
- `add-claim-status`: Register a new claim status across domain models, localization dictionaries, workflow steppers, and dashboard color maps. (`.github/skills/add-claim-status/SKILL.md`)
- `add-detail-panel`: Add a presentational or action-oriented subpanel component for the claim detail view. (`.github/skills/add-detail-panel/SKILL.md`)
- `add-api-command-client`: Add a mutation command to ClaimsApi, invoke it from a component with error handling, and add an injectable API client service for HTTP commands. (`.github/skills/add-api-command-client/SKILL.md`)
- `add-claim-action`: Define a workflow action in the claim domain models with UI metadata, localization, input requirements, and tone configuration. (`.github/skills/add-claim-action/SKILL.md`)
- `add-locale-pipe`: Implement an Angular standalone formatting pipe that leverages injected LOCALE_ID for localized rendering. (`.github/skills/add-locale-pipe/SKILL.md`)
