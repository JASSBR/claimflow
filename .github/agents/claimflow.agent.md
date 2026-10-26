---
name: claimflow
description: "Makes changes in claimflow the way this repository makes them, starting from the matching skill and the rules the estate holds about the files it touches."
tools: ['read', 'edit', 'search', 'gradera/draft_requirements', 'gradera/get_asset_content', 'gradera/get_asset_profile', 'gradera/get_assets', 'gradera/get_backlog_tree', 'gradera/get_bundle', 'gradera/get_context_object', 'gradera/get_facts_for_symbol']
---

# claimflow

Work in this order for every change:

1. Find the skill below whose task matches the change, and read it in full before editing anything. If none matches, say so and work from the nearest one.
2. Open the file its **Start here** section names and follow that example's shape.
3. Follow its **Workflow** and **Checklist**; every step cites the line it comes from.
4. Keep every rule in **Rules this change touches** holding. A rule marked not yet reviewed is the estate's best reading, not a decision: say when a change contradicts one.
5. Check the files under **Files you will probably also touch**.
6. When a skill does not answer a question about this codebase, ask the Gradera tools (`draft_requirements`, `get_asset_content`, `get_asset_profile`, `get_assets`, `get_backlog_tree`, `get_bundle`, `get_context_object`, `get_facts_for_symbol`) before guessing, and cite what they return.
7. Before finishing, run: `dotnet build ClaimFlow.slnx`; `dotnet test ClaimFlow.slnx`.
8. Summarise what changed and cite the skill and the lines you followed.

## Skills

| Skill | Use it to | File |
|---|---|---|
| `add-api-endpoint` | Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results. | `.github/skills/add-api-endpoint/SKILL.md` |
| `add-claim-detail-panel` | Add a claim detail facet panel component in the Angular web client to inspect or process claim attachments and analysis. | `.github/skills/add-claim-detail-panel/SKILL.md` |
| `add-routed-page` | Register lazy-loaded page routes under the portal router with authentication guards. | `.github/skills/add-routed-page/SKILL.md` |
| `add-domain-event-handler` | Implement and register an outbox domain event handler to process committed domain events. | `.github/skills/add-domain-event-handler/SKILL.md` |
| `add-portal-icon` | Add inline SVG stroke icons to the portal icon dictionary on the 24x24 grid. | `.github/skills/add-portal-icon/SKILL.md` |
| `define-domain-errors` | Declare structured domain error definitions for an aggregate or module as static readonly Error members using factory methods on Error. | `.github/skills/define-domain-errors/SKILL.md` |
| `map-api-error` | Map a backend RFC 9457 domain error code to a localized user-facing message in the web portal error registry. | `.github/skills/map-api-error/SKILL.md` |
| `test-domain-entities` | Write unit tests for domain entities and aggregates verifying factory creation, business invariants, and domain events. | `.github/skills/test-domain-entities/SKILL.md` |
| `add-aggregate-root` | Define a domain aggregate root inheriting from AggregateRoot<TId> that buffers domain events for outbox persistence. | `.github/skills/add-aggregate-root/SKILL.md` |
| `add-domain-aggregate` | Implement a domain aggregate root inheriting from AggregateRoot<TId> with a private persistence constructor, factory method returning Result<T>, and static domain error definitions. | `.github/skills/add-domain-aggregate/SKILL.md` |
| `add-localized-ui-string` | Add user-facing text to Angular templates with explicit @@ message IDs and provide corresponding English translations in the translation catalog. | `.github/skills/add-localized-ui-string/SKILL.md` |
| `bind-http-resource` | Bind signal-driven read resources in Angular components using httpResource with realtime reloads. | `.github/skills/bind-http-resource/SKILL.md` |
| `configure-module-outbox` | Configure a module DbContext and service collection to persist and dispatch outbox messages for domain events. | `.github/skills/configure-module-outbox/SKILL.md` |
| `add-chart-component` | Add a dependency-free SVG or CSS visualization chart component in shared/charts with signal inputs and OnPush change detection. | `.github/skills/add-chart-component/SKILL.md` |
| `add-claim-status` | Register a new claim status across domain models, localization dictionaries, workflow steppers, and dashboard color maps. | `.github/skills/add-claim-status/SKILL.md` |
| `add-detail-panel` | Add a presentational or action-oriented subpanel component for the claim detail view. | `.github/skills/add-detail-panel/SKILL.md` |
| `add-domain-entity` | Implement domain entities and aggregate roots in Domain projects referencing only SharedKernel with private materialization constructors. | `.github/skills/add-domain-entity/SKILL.md` |
| `register-modular-slice` | Configure and register a modular vertical slice with DI, database pooling, outbox, and endpoint routing. | `.github/skills/register-modular-slice/SKILL.md` |
| `map-result-to-problem` | Convert a domain or application failure Result into an RFC 9457 HTTP ProblemDetails response. | `.github/skills/map-result-to-problem/SKILL.md` |
| `add-api-command-client` | Add a mutation command to ClaimsApi, invoke it from a component with error handling, and add an injectable API client service for HTTP commands. | `.github/skills/add-api-command-client/SKILL.md` |
| `add-claim-action` | Define a workflow action in the claim domain models with UI metadata, localization, input requirements, and tone configuration. | `.github/skills/add-claim-action/SKILL.md` |
| `add-workflow-transition` | Add or extend a state machine transition on the Claim aggregate root with audit history and authority checks. | `.github/skills/add-workflow-transition/SKILL.md` |
| `define-module-permissions` | Create strongly typed module configuration options with a standard section name and register them in the module's dependency injection container. | `.github/skills/define-module-permissions/SKILL.md` |
| `register-domain-event` | Define a domain event implementing IDomainEvent with an occurrence timestamp for aggregate state changes. | `.github/skills/register-domain-event/SKILL.md` |
| `add-locale-pipe` | Implement an Angular standalone formatting pipe that leverages injected LOCALE_ID for localized rendering. | `.github/skills/add-locale-pipe/SKILL.md` |
