---
name: add-portal-icon
description: "Add inline SVG stroke icons to the portal icon dictionary on the 24x24 grid."
---

> Evidence: value gate verified the facts below against this repository.

Add inline SVG stroke icons to the portal icon dictionary on the 24x24 grid.

Applies in: `claimflow-web`.

## What is specific to this repository

- Portal icons are stroke icons drawn on a 24×24 grid in the style of Lucide without an icon font or network request. [web/src/app/shared/icon.ts:L3-L3]
- The IconName type is derived from the keys of the PATHS dictionary. [web/src/app/shared/icon.ts:L37-L37]
- The icon component uses the app-icon selector. [web/src/app/shared/icon.ts:L40-L40]
- The Icon component requires an icon name input of type IconName. [web/src/app/shared/icon.ts:L65-L65]

## Rules this change touches

- [Invariant] While an authentication request for a persona is in flight, the pending signal stores that persona's identifier and is cleared only if the login operation encounters an error. (approved, conviction 1.00, web/src/app/pages/login/login.ts:L78-L88)
- [Invariant] The rendered SVG element always marks aria-hidden="true", renders with stroke currentColor and stroke-width 1.9, and sizes both width and height to size(). (approved, conviction 1.00, web/src/app/shared/icon.ts:L32-L47)
- [BusinessRule] When mapping a persona's assigned roles to a localized display label, if the persona includes the 'claims.manager' role, the label is 'Responsable'; otherwise if it includes 'claims.auditor', the label is 'Lecture seule'; otherwise the default label is 'Gestionnaire'. (approved, conviction 1.00, web/src/app/pages/login/login.ts:L72-L76)
- [InformalDecision] Icons are implemented as inline SVG path strings drawn on a 24×24 grid in the Lucide style, avoiding the need for an icon font or external network requests. (approved, conviction 1.00, web/src/app/shared/icon.ts:L3-L33)

## Context

- The enter method triggers the login sequence for a given persona: it marks the persona id as pending, clears any previous error, requests a session token via Auth.loginAs(persona.id), navigates to '/dashboard' on success, or sets the error signal to the first message extracted by problemMessages(error) and resets pending on failure. [web/src/app/pages/login/login.ts:L78-L88]
- Highlight defines the structure for marketing and architecture feature highlights displayed on the login page, containing an icon name (constrained by IconName), a localized title, and localized description text. [web/src/app/pages/login/login.ts:L20-L24]
- Shell is the root layout component hosting the application navigation, user profile/persona selector, theme toggle, and main router outlet. It fetches user capabilities from `/api/claims/capabilities` and lazily fetches demo personas from `/api/auth/personas` when the persona switch menu is opened. [web/src/app/layout/shell.ts:L20-L36]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/app.routes.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/locale.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/realtime/claims-realtime.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/theme.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-declare/claim-declare.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/shared/icon.ts` [web/src/app/shared/icon.ts:L2-L41] is the worked example for this change, as it stands in the repository:

```typescript

// Stroke icons on a 24×24 grid, drawn in the style of Lucide (ISC). Inline: no icon font, no network request.
const PATHS = {
  dashboard: 'M3 3h7v9H3zM14 3h7v5h-7zM14 12h7v9h-7zM3 16h7v5H3z',
  folder: 'M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z',
  plus: 'M12 5v14M5 12h14',
  file: 'M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8zM14 3v5h5M9 13h6M9 17h6',
  image:
    'M5 4h14a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1zM8.5 10a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3zM20 15l-5-5L5 20',
  upload: 'M12 16V4M7 9l5-5 5 5M4 16v3a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1v-3',
  sparkles:
    'M12 3l1.9 5.1L19 10l-5.1 1.9L12 17l-1.9-5.1L5 10l5.1-1.9zM19 15l.9 2.1L22 18l-2.1.9L19 21l-.9-2.1L16 18l2.1-.9z',
  sun: 'M12 16a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4',
  moon: 'M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z',
  logout: 'M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9',
  shield: 'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10zM9 12l2 2 4-4',
  check: 'M20 6 9 17l-5-5',
  x: 'M18 6 6 18M6 6l12 12',
  alert:
    'M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0zM12 9v4M12 17h.01',
  clock: 'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20zM12 6v6l4 2',
  search: 'M11 19a8 8 0 1 0 0-16 8 8 0 0 0 0 16zM21 21l-4.3-4.3',
  back: 'M19 12H5M12 19l-7-7 7-7',
  external: 'M15 3h6v6M10 14 21 3M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6',
  users:
    'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8',
  euro: 'M18 7a7 7 0 1 0 0 10M4 10h10M4 14h10',
  bolt: 'M13 2 3 14h9l-1 8 10-12h-9z',
  github:
    'M9 19c-5 1.5-5-2.5-7-3m14 6v-3.9a3.4 3.4 0 0 0-.9-2.6c3.1-.4 6.4-1.5 6.4-7a5.4 5.4 0 0 0-1.5-3.8 5 5 0 0 0-.1-3.8s-1.2-.4-3.9 1.5a13.4 13.4 0 0 0-7 0C6.3 1.6 5.1 2 5.1 2a5 5 0 0 0-.1 3.8A5.4 5.4 0 0 0 3.5 9.6c0 5.4 3.3 6.6 6.4 7a3.4 3.4 0 0 0-.9 2.6V23',
  activity: 'M22 12h-4l-3 9L9 3l-3 9H2',
  lock: 'M5 11h14v10H5zM8 11V7a4 4 0 0 1 8 0v4',
  chevron: 'm9 18 6-6-6-6',
} as const;

export type IconName = keyof typeof PATHS;

@Component({
  selector: 'app-icon',
  template: `<svg
```

## The same change elsewhere in this repository

- `web/src/app/pages/login/login.ts` [web/src/app/pages/login/login.ts:L14-L14]: `readonly icon: IconName;`
- `web/src/app/pages/login/login.ts` [web/src/app/pages/login/login.ts:L21-L21]: `imports: [Avatar, Icon, CurrencyPipe],`
- `web/src/app/pages/login/login.ts` [web/src/app/pages/login/login.ts:L38-L38]: `icon: 'shield',`
- `web/src/app/layout/shell.ts` [web/src/app/layout/shell.ts:L17-L17]: `imports: [RouterOutlet, RouterLink, RouterLinkActive, Avatar, Icon, CurrencyPipe],`

## Patterns

Required:
- `Icon`
- `IconName`
- `PATHS`
Forbidden:
- Avoid `lucide-angular` in this change.

## Workflow

1. Obtain the stroke path data formatted for a 24x24 viewBox in Lucide style. [web/src/app/shared/icon.ts:L42-L42]
2. Open web/src/app/shared/icon.ts. [web/src/app/shared/icon.ts:L5-L5]
3. Add the icon name and path string to the PATHS dictionary. [web/src/app/shared/icon.ts:L4-L4]
4. Verify that IconName automatically exposes the new key for use in <app-icon [name]="...">. [web/src/app/shared/icon.ts:L37-L37]
5. Design or obtain an inline SVG path on a 24x24 grid matching Lucide stroke style. [web/src/app/shared/icon.ts:L3-L3]
6. Add the key and path string into the PATHS constant in web/src/app/shared/icon.ts. [web/src/app/shared/icon.ts:L5-L5]
7. Ensure IconName automatically expands to include the new icon name via keyof typeof PATHS. [web/src/app/shared/icon.ts:L37-L37]
8. Import Icon in standalone component imports arrays and render with <app-icon [name]="..."/> [web/src/app/shared/icon.ts:L64-L64]

## Checklist

- [ ] The path is designed for stroke rendering (fill="none", stroke="currentColor", stroke-width=1.9). [web/src/app/shared/icon.ts:L56-L56]
- [ ] The icon key name in PATHS is clean kebab-case or single-word identifier. [web/src/app/shared/icon.ts:L4-L4]
- [ ] IconName type automatically infers the new icon key. [web/src/app/shared/icon.ts:L37-L37]
- [ ] Is the SVG path formatted for a 24x24 grid in Lucide style (stroke-width 1.9)? [web/src/app/shared/icon.ts:L3-L3]
- [ ] Is the icon name added to PATHS in web/src/app/shared/icon.ts? [web/src/app/shared/icon.ts:L5-L5]
- [ ] Is the Icon component included in the imports array of the consuming standalone component? [web/src/app/shared/icon.ts:L64-L64]
