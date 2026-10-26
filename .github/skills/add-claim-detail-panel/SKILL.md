---
name: add-claim-detail-panel
description: "Add a claim detail facet panel component in the Angular web client to inspect or process claim attachments and analysis."
---

> Evidence: value gate verified the facts below against this repository.

Add a claim detail facet panel component in the Angular web client to inspect or process claim attachments and analysis.

Applies in: `claimflow-web`.

## What is specific to this repository

- Claim detail panel components configure OnPush change detection and import DatePipe, DecimalPipe, and Icon from shared components. [web/src/app/pages/claim-detail/analysis-panel.ts:L30-L36]
- Claim detail panels declare a required claimId string input alongside an optional DocumentSettings configuration input. [web/src/app/pages/claim-detail/documents-panel.ts:L21-L23]
- AnalysisPanel defines four localized progression step labels to indicate analysis progress to the user. [web/src/app/pages/claim-detail/analysis-panel.ts:L23-L28]

## Rules this change touches

- [BusinessRule] Running or re-running the AI claim analysis requires at least one document attached to the claim; if documentCount() is 0, the action button is disabled and an informational notice prompts the user to add at least one document. (approved, conviction 1.00, web/src/app/pages/claim-detail/analysis-panel.html:L90-L106)
- [InformalDecision] ESLint flat configuration defines linting for TypeScript files ('**/*.ts') by extending ESLint recommended, typescript-eslint recommended and stylistic configs, and angular-eslint tsRecommended rules, using angular.processInlineTemplates processor. (approved, conviction 1.00, web/eslint.config.js:L7-L16)

## Context

- The openSource method opens the cited document via DocumentsApi.open, passing the claim ID, the citation's documentId, and its startPage. [web/src/app/pages/claim-detail/analysis-panel.ts:L70-L72]
- The ngOnDestroy lifecycle hook clears the active ticker interval to prevent background timer leaks when the component is unmounted. [web/src/app/pages/claim-detail/analysis-panel.ts:L94-L96]
- The ESLint configuration exports a flat config array using defineConfig to define linting rules for TypeScript files (**/*.ts) and Angular HTML templates (**/*.html), combining base ESLint recommended rules, typescript-eslint recommended and stylistic rules, and angular-eslint recommended rules. [web/eslint.config.js:L7-L38]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/claims/claim-labels.ts` | referenced by or referencing this skill's files |
| `web/src/app/claims/claim.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/toast.ts` | referenced by or referencing this skill's files |
| `web/src/app/documents/analysis-parser.ts` | referenced by or referencing this skill's files |
| `web/src/app/documents/documents-api.ts` | referenced by or referencing this skill's files |
| `web/src/app/documents/documents.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/analysis-panel.spec.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/claim-detail.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/pages/claim-detail/analysis-panel.html` [web/src/app/pages/claim-detail/analysis-panel.html:L1-L40] is the worked example for this change, as it stands in the repository:

```html
<section class="card ai" aria-labelledby="ai-title">
  <div class="card-title">
    <h2 id="ai-title">
      <span class="spark"><app-icon name="sparkles" [size]="16" /></span>
      <span i18n="@@analysis.title">Revue IA du dossier</span>
    </h2>
    @if (analysis(); as a) {
      <span class="chip">{{ a.model }}</span>
    }
  </div>

  @if (settings() && !settings()!.aiEnabled) {
    <p class="note" i18n="@@analysis.disabled">
      L'assistant IA n'est pas activé sur cet environnement (clé API absente). Le reste de
      l'application fonctionne normalement ; l'activation est décrite dans le README.
    </p>
  } @else if (running()) {
    <ol class="progress" aria-live="polite">
      @for (label of steps; track label) {
        <li [class.done]="$index < step()" [class.current]="$index === step()">
          <span class="bullet">
            @if ($index < step()) {
              <app-icon name="check" [size]="12" />
            }
          </span>
          {{ label }}
        </li>
      }
    </ol>
    <div class="skeleton line" style="width: 90%"></div>
    <div class="skeleton line" style="width: 75%"></div>
    <div class="skeleton line" style="width: 82%"></div>
  } @else if (analysis(); as a) {
    @if (a.refused) {
      <p class="note" i18n="@@analysis.refused">
        Le modèle a décliné cette analyse. Les pièces peuvent être examinées manuellement.
      </p>
    } @else {
      <article class="report">
        @for (block of blocks(); track $index) {
```

## The same change elsewhere in this repository

- `web/src/app/pages/claim-detail/analysis-panel.ts` [web/src/app/pages/claim-detail/analysis-panel.ts:L32-L32]: `  imports: [DatePipe, DecimalPipe, Icon],`
- `web/src/app/pages/claim-detail/analysis-panel.ts` [web/src/app/pages/claim-detail/analysis-panel.ts:L40-L40]: `  readonly claimId = input.required<string>();`
- `web/src/app/pages/claim-detail/analysis-panel.ts` [web/src/app/pages/claim-detail/analysis-panel.ts:L42-L42]: `readonly settings = input<DocumentSettings | undefined>();`
- `web/src/app/pages/claim-detail/analysis-panel.ts` [web/src/app/pages/claim-detail/analysis-panel.ts:L87-L87]: `      this.error.set(problemMessages(error).join(' '));`
- `web/src/app/pages/claim-detail/documents-panel.ts` [web/src/app/pages/claim-detail/documents-panel.ts:L21-L21]: `readonly claimId = input.required<string>();`
- `web/src/app/pages/claim-detail/documents-panel.ts` [web/src/app/pages/claim-detail/documents-panel.ts:L23-L23]: `readonly settings = input<DocumentSettings | undefined>();`
- `web/src/app/pages/claim-detail/documents-panel.ts` [web/src/app/pages/claim-detail/documents-panel.ts:L59-L59]: `message: problemMessages(error).join(' '),`
- `web/src/app/pages/claim-detail/claim-timeline.ts` [web/src/app/pages/claim-detail/claim-timeline.ts:L88-L88]: `changeDetection: ChangeDetectionStrategy.OnPush,`
- `web/src/app/pages/claim-detail/claim-timeline.ts` [web/src/app/pages/claim-detail/claim-timeline.ts:L91-L91]: `readonly claim = input.required<ClaimDetails>();`
- `web/eslint.config.js` [web/eslint.config.js:L18-L18]: `// ECC Angular rule "OnPush on every component", enforced instead of remembered.`
- `web/eslint.config.js` [web/eslint.config.js:L19-L19]: `'@angular-eslint/prefer-on-push-component-change-detection': 'error',`

## Patterns

Required:
- `@angular-eslint/prefer-on-push-component-change-detection`
- `ChangeDetectionStrategy.OnPush`
- `app-icon`
- `aria-labelledby`
- `card-title`
- `firstValueFrom`
- `input.required`
- `problemMessages`
- `role="alert"`
Forbidden:
- Avoid `toPromise` in this change.

## Workflow

1. Create the panel component in web/src/app/pages/claim-detail/ with OnPush change detection and HTML/SCSS template bindings. [web/src/app/pages/claim-detail/analysis-panel.ts:L32-L32]
2. Import standard pipes (DatePipe, DecimalPipe) and shared Icon in the @Component imports array. [web/src/app/pages/claim-detail/analysis-panel.ts:L1-L1]
3. Bind claimId as input.required<string>() alongside required state inputs and completion outputs. [web/src/app/pages/claim-detail/analysis-panel.ts:L40-L40]
4. Inject relevant API services (such as DocumentsApi) and ToastService using inject(). [web/src/app/pages/claim-detail/analysis-panel.ts:L13-L13]
5. Invoke API methods returning Observables using firstValueFrom(). [web/src/app/pages/claim-detail/analysis-panel.ts:L12-L12]
6. Catch exceptions and format RFC 7807 problem details using problemMessages(error).join(' '). [web/src/app/pages/claim-detail/analysis-panel.ts:L21-L21]
7. Declare claimId as input.required<string>() and any configuration inputs like settings as input<DocumentSettings | undefined>(). [web/src/app/pages/claim-detail/analysis-panel.ts:L40-L40]

## Checklist

- [ ] Enable ChangeDetectionStrategy.OnPush on the @Component definition. [web/src/app/pages/claim-detail/analysis-panel.ts:L3-L3]
- [ ] Include shared utility imports like DatePipe, DecimalPipe, and Icon where required. [web/src/app/pages/claim-detail/analysis-panel.ts:L1-L1]
- [ ] Declare input.required<string>() for the claimId parameter. [web/src/app/pages/claim-detail/analysis-panel.ts:L40-L40]
- [ ] Use problemMessages(error) from shared problem-details to format API rejection errors. [web/src/app/pages/claim-detail/analysis-panel.ts:L21-L21]
- [ ] Ensure change detection is configured for OnPush as required by eslint.config.js rules. [web/src/app/pages/claim-detail/analysis-panel.ts:L35-L35]
- [ ] Component uses ChangeDetectionStrategy.OnPush [web/src/app/pages/claim-detail/analysis-panel.ts:L3-L3]
