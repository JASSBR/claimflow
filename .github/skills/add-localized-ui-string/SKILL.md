---
name: add-localized-ui-string
description: "Add user-facing text to Angular templates with explicit @@ message IDs and provide corresponding English translations in the translation catalog."
---

> Evidence: value gate verified the facts below against this repository.

Add user-facing text to Angular templates with explicit @@ message IDs and provide corresponding English translations in the translation catalog.

Applies in: `claimflow-web`.

## What is specific to this repository

- The repository defines an i18n npm script that extracts strings to src/locale and executes tools/build-translation.py. [web/package.json:L14-L14]
- The start:en npm script serves the application with the English configuration on port 4300 by default. [web/package.json:L15-L15]
- Templates author user-facing text in French with explicit i18n message IDs prefixed with @@. [web/src/app/layout/shell.html:L10-L10]

## Rules this change touches

- [BusinessRule] Running or re-running the AI claim analysis requires at least one document attached to the claim; if documentCount() is 0, the action button is disabled and an informational notice prompts the user to add at least one document. (approved, conviction 1.00, web/src/app/pages/claim-detail/analysis-panel.html:L90-L106)
- [BusinessRule] Prescription biennale : un sinistre survenu il y a plus de deux ans ne peut plus être déclaré (art. L114-1 du Code des assurances). (approved, conviction 1.00, web/src/app/pages/claim-declare/claim-declare.html:L67-L73)

## Context

- The shell layout template defines the main application shell with a persistent sidebar, a topbar containing status indicators and actions, an optional read-only banner, and a main content area housing the Angular router-outlet. [web/src/app/layout/shell.html:L1-L107]
- The web package is configured as a private npm package named 'claimflow-web' at version '0.0.0', specifying 'npm@11.19.0' as its package manager. [web/package.json:L1-L17]
- The file web/tools/translations.en.json provides the comprehensive English localization string dictionary for the ClaimFlow web application. Translation keys are categorized with dot-delimited namespaces covering user actions, AI file reviews, UI dashboard elements, claim declaration input constraints, claim detail inspection, evidence document uploads, domain error messages, portfolio KPIs, login screen copy, role definitions, and claim lifecycle statuses. [web/tools/translations.en.json:L1-L147]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/pages/claim-detail/claim-detail.html` | changed together in 3 of 3 commits (100%) |

## Start here

`web/package.json` [web/package.json:L11-L48] is the worked example for this change, as it stands in the repository:

```json
    "lint": "ng lint",
    "format:check": "prettier --check \"src/**/*.{ts,html,scss}\"",
    "e2e": "playwright test",
    "i18n": "ng extract-i18n --output-path src/locale && python3 tools/build-translation.py",
    "start:en": "ng serve -c en --port ${PORT:-4300}"
  },
  "private": true,
  "packageManager": "npm@11.19.0",
  "dependencies": {
    "@angular/common": "^22.2.0",
    "@angular/compiler": "^22.2.0",
    "@angular/core": "^22.2.0",
    "@angular/forms": "^22.2.0",
    "@angular/platform-browser": "^22.2.0",
    "@angular/router": "^22.2.0",
    "@fontsource-variable/jetbrains-mono": "^5.3.0",
    "@fontsource-variable/plus-jakarta-sans": "^5.3.0",
    "@microsoft/signalr": "^10.0.11",
    "rxjs": "~7.8.0",
    "tslib": "^2.3.0"
  },
  "devDependencies": {
    "@angular/build": "^22.2.1",
    "@angular/cli": "^22.2.1",
    "@angular/compiler-cli": "^22.2.0",
    "@angular/localize": "^22.2.1",
    "@eslint/js": "^10.0.1",
    "@playwright/test": "^1.63.0",
    "@vitest/coverage-v8": "^5.0.3",
    "angular-eslint": "22.5.0",
    "eslint": "^10.9.1",
    "jsdom": "^30.0.0",
    "prettier": "^3.8.1",
    "typescript": "~6.0.2",
    "typescript-eslint": "8.69.0",
    "vitest": "^5.0.0"
  }
}
```

## The same change elsewhere in this repository

- `web/src/app/layout/shell.html` [web/src/app/layout/shell.html:L8-L8]: `<nav i18n-aria-label="@@nav.main" aria-label="Navigation principale">`
- `web/src/app/pages/claim-declare/claim-declare.html` [web/src/app/pages/claim-declare/claim-declare.html:L7-L7]: `    <span class="eyebrow" i18n="@@declare.eyebrow">Première déclaration</span>`
- `web/src/app/pages/claim-detail/analysis-panel.html` [web/src/app/pages/claim-detail/analysis-panel.html:L5-L5]: `      <span i18n="@@analysis.title">Revue IA du dossier</span>`
- `web/src/app/pages/claim-detail/analysis-panel.html` [web/src/app/pages/claim-detail/analysis-panel.html:L13-L13]: `<p class="note" i18n="@@analysis.disabled">`
- `web/tools/translations.en.json` [web/tools/translations.en.json:L14-L14]: `"analysis.disabled": "The AI assistant is not enabled on this environment (no API key). Everything else works normally; the README explains how to enable it.",`
- `web/tools/translations.en.json` [web/tools/translations.en.json:L28-L28]: `"analysis.title": "AI file review",`

## Patterns

Required:
- `build-translation.py`
- `i18n="@@`
- `start:en`

## Workflow

1. Tag the element in the Angular HTML template with an i18n attribute using an explicit ID prefixed with @@ (e.g. i18n="@@analysis.title"). [web/src/app/pages/claim-detail/analysis-panel.html:L5-L5]
2. Provide the default French text directly inside the template element. [web/tools/translations.en.json:L82-L82]
3. Run npm run i18n to invoke ng extract-i18n and tools/build-translation.py. [web/package.json:L14-L14]

## Checklist

- [ ] Verify that template text uses explicit localization attributes (i18n="@@id", i18n-aria-label="@@id", or i18n-placeholder="@@id") with default text in French. [web/tools/translations.en.json:L82-L82]
- [ ] Verify that an entry with the exact ID (omitting the @@ prefix) exists in web/tools/translations.en.json. [web/tools/translations.en.json:L14-L14]
- [ ] Run npm run i18n to extract messages and build the translation catalog via tools/build-translation.py. [web/package.json:L14-L14]
- [ ] Ensure the default text in HTML templates is written in French. [web/tools/translations.en.json:L82-L82]
- [ ] Verify that the matching key in web/tools/translations.en.json does not include the leading @@. [web/tools/translations.en.json:L14-L14]
- [ ] Verify that translation placeholders follow the {INTERPOLATION} naming convention if dynamic bindings are used. [web/tools/translations.en.json:L15-L15]
