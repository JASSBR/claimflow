## Estate grounding (Gradera)

The estate knowledge of `claimflow` is held by the Gradera Context Engine and reached through the `gradera` MCP server this repository's kit configures. Read it before you write, and keep to it:

- Knowledge in state `approved` or `validated` is a constraint. Write nothing that contradicts it. When the request does, say which object it contradicts and stop for a decision.
- Knowledge in state `inferred` is the estate's best reading, not a decision. Use it as a question to confirm, never as a fact.
- Cite every estate fact you use with its object id and state, as `(gradera:<object id>, approved)`.
- When the `gradera` server cannot be reached, say so where the estate section belongs and carry on. Never write an estate fact the server did not return.

### While you write the tasks

1. For each task that changes code, name the kit skill it follows and the files the plan's Estate Impact says it reaches.
2. Add a task for every test or check an applying rule asks for.

| Skill | Use it to | File |
|---|---|---|
| `add-aggregate-root` | Define a domain aggregate root inheriting from AggregateRoot<TId> that buffers domain events for outbox persistence. | `.github/skills/add-aggregate-root/SKILL.md` |
| `add-api-command-client` | Add a mutation command to ClaimsApi, invoke it from a component with error handling, and add an injectable API client service for HTTP commands. | `.github/skills/add-api-command-client/SKILL.md` |
| `add-api-endpoint` | Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results. | `.github/skills/add-api-endpoint/SKILL.md` |
| `add-chart-component` | Add a dependency-free SVG or CSS visualization chart component in shared/charts with signal inputs and OnPush change detection. | `.github/skills/add-chart-component/SKILL.md` |
| `add-claim-action` | Define a workflow action in the claim domain models with UI metadata, localization, input requirements, and tone configuration. | `.github/skills/add-claim-action/SKILL.md` |
| `add-claim-detail-panel` | Add a claim detail facet panel component in the Angular web client to inspect or process claim attachments and analysis. | `.github/skills/add-claim-detail-panel/SKILL.md` |
| `add-claim-status` | Register a new claim status across domain models, localization dictionaries, workflow steppers, and dashboard color maps. | `.github/skills/add-claim-status/SKILL.md` |
| `add-detail-panel` | Add a presentational or action-oriented subpanel component for the claim detail view. | `.github/skills/add-detail-panel/SKILL.md` |
| `add-domain-aggregate` | Implement a domain aggregate root inheriting from AggregateRoot<TId> with a private persistence constructor, factory method returning Result<T>, and static domain error definitions. | `.github/skills/add-domain-aggregate/SKILL.md` |
| `add-domain-entity` | Implement domain entities and aggregate roots in Domain projects referencing only SharedKernel with private materialization constructors. | `.github/skills/add-domain-entity/SKILL.md` |
| `add-domain-event-handler` | Implement and register an outbox domain event handler to process committed domain events. | `.github/skills/add-domain-event-handler/SKILL.md` |
| `add-locale-pipe` | Implement an Angular standalone formatting pipe that leverages injected LOCALE_ID for localized rendering. | `.github/skills/add-locale-pipe/SKILL.md` |
| `add-localized-ui-string` | Add user-facing text to Angular templates with explicit @@ message IDs and provide corresponding English translations in the translation catalog. | `.github/skills/add-localized-ui-string/SKILL.md` |
| `add-portal-icon` | Add inline SVG stroke icons to the portal icon dictionary on the 24x24 grid. | `.github/skills/add-portal-icon/SKILL.md` |
| `add-routed-page` | Register lazy-loaded page routes under the portal router with authentication guards. | `.github/skills/add-routed-page/SKILL.md` |
| `add-workflow-transition` | Add or extend a state machine transition on the Claim aggregate root with audit history and authority checks. | `.github/skills/add-workflow-transition/SKILL.md` |
| `bind-http-resource` | Bind signal-driven read resources in Angular components using httpResource with realtime reloads. | `.github/skills/bind-http-resource/SKILL.md` |
| `configure-module-outbox` | Configure a module DbContext and service collection to persist and dispatch outbox messages for domain events. | `.github/skills/configure-module-outbox/SKILL.md` |
| `define-domain-errors` | Declare structured domain error definitions for an aggregate or module as static readonly Error members using factory methods on Error. | `.github/skills/define-domain-errors/SKILL.md` |
| `define-module-permissions` | Create strongly typed module configuration options with a standard section name and register them in the module's dependency injection container. | `.github/skills/define-module-permissions/SKILL.md` |
| `map-api-error` | Map a backend RFC 9457 domain error code to a localized user-facing message in the web portal error registry. | `.github/skills/map-api-error/SKILL.md` |
| `map-result-to-problem` | Convert a domain or application failure Result into an RFC 9457 HTTP ProblemDetails response. | `.github/skills/map-result-to-problem/SKILL.md` |
| `register-domain-event` | Define a domain event implementing IDomainEvent with an occurrence timestamp for aggregate state changes. | `.github/skills/register-domain-event/SKILL.md` |
| `register-modular-slice` | Configure and register a modular vertical slice with DI, database pooling, outbox, and endpoint routing. | `.github/skills/register-modular-slice/SKILL.md` |
| `test-domain-entities` | Write unit tests for domain entities and aggregates verifying factory creation, business invariants, and domain events. | `.github/skills/test-domain-entities/SKILL.md` |
