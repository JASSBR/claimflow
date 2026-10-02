# 0007 — Zoneless Angular, signals end-to-end, server-driven workflow

- **Status:** Accepted · 2026-10-02

## Decision

- **Angular 22, zoneless**, every component `OnPush` (enforced by ESLint), standalone, lazy-loaded routes.
- **Reads are `httpResource`** declared in the component that renders them. **Commands** go through `ClaimsApi`.
- **Signal Forms** (`@angular/forms/signals`, stable since v22) for the declaration form: the schema is the single source
  of client-side rules (Angular refuses `min`/`max` attributes on bound fields), the API remains the authority and its
  problem details are displayed as-is.
- **Realtime without subscriptions**: `ClaimsRealtime` exposes the last SignalR notification as a signal, and
  `reloadWhen(trigger, ...resources)` reloads the resources on screen when it changes; the detail page filters through a
  `computed` so another claim's notification triggers nothing. It calls `reload()` rather than making the request depend
  on the notification: a changed request resets the resource to "loading" with no value, which flashed a skeleton and
  destroyed open forms mid-action — a bug the Playwright suite caught intermittently before it was fixed.
- **Server-driven workflow**: the API returns `allowedActions` computed from the domain's workflow table; the UI renders
  one button per allowed action and contains **no workflow logic**. Changing the workflow is a backend-only change.
- **Internationalisation (FR/EN) with Angular's built-in i18n**: `$localize` and `i18n` attributes with stable ids
  (`@@decision.fourEyes`), one compiled build per language served under `/fr/` and `/en/` — translations cost nothing at
  runtime. The API returns stable error **codes**; the UI owns their wording in each language and falls back to the
  server's description for a code it does not know yet. `tools/build-translation.py` regenerates `messages.en.xlf` from
  the extraction and fails on any missing translation or unknown placeholder.
- The hub connection is injected through an `InjectionToken` factory, so the service is testable without a server.

## Consequences

- ✅ No `subscribe`/`unsubscribe` in components, no manual change detection.
- ✅ UI and API can never disagree about which transitions are legal.
- ⚠️ Every notification causes a refetch rather than a local patch: simpler and always consistent, slightly chattier.
