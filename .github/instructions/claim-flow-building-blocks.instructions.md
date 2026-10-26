---
applyTo: 'src/ClaimFlow.BuildingBlocks/**'
---

# ClaimFlow.BuildingBlocks

`src/ClaimFlow.BuildingBlocks/` is a dotnet project of this repository, declared by `src/ClaimFlow.BuildingBlocks/ClaimFlow.BuildingBlocks.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/ClaimFlow.BuildingBlocks/ClaimFlow.BuildingBlocks.csproj`

## Depends on

Depends on `claim-flow-shared-kernel` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] A failed Result instance must contain at least one error in its Errors collection when converted via ToProblem. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L17-L17]
- [Invariant] The modelBuilder parameter passed to ApplyOutbox must not be null; an ArgumentNullException is thrown if it is null. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxModelBuilderExtensions.cs:L8-L10]
- [Invariant] All event types contributed to the OutboxEventRegistry must implement the IDomainEvent interface, or an ArgumentException is thrown. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs:L16-L25]
- [Invariant] ClaimFlow.BuildingBlocks must never be referenced by any Domain project in the solution. [src/ClaimFlow.BuildingBlocks/ClaimFlow.BuildingBlocks.csproj:L2-L2]
- [Invariant] The configuration argument passed to AddOutbox<TDbContext> must not be null, throwing ArgumentNullException if it is. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxServiceCollectionExtensions.cs:L11-L18]
- [Invariant] Upon successful dispatch and execution of domain event handlers for an outbox message, ProcessedAt must be set to the current UTC timestamp and LastError must be set to null. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L128-L131]
- [Invariant] Handlers implementing `IDomainEventHandler<in TEvent>` must be idempotent because domain event delivery via the outbox is at-least-once. [src/ClaimFlow.BuildingBlocks/Outbox/IDomainEventHandler.cs:L5-L5]
- [Invariant] A Result converted via ToProblem must not be null and must not represent a successful outcome (result.IsSuccess must be false). Attempting to convert a successful result throws an InvalidOperationException. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L10-L15]
- [BusinessRule] When outbox message dispatch fails with an exception not caused by stopping cancellation, the message's Attempts counter is incremented by 1 and LastError is set to the exception message truncated to a maximum of 2000 characters. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L133-L141]
- [BusinessRule] An authenticated ClaimsPrincipal must contain a 'sub' claim; if missing, an InvalidOperationException is thrown with message "Authenticated principal has no 'sub' claim.". [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L16-L18]
- [InformalDecision] Standard OIDC claim names ("sub", "name", and "role") are maintained as raw constant strings rather than mapped inbound claims, maintaining compatibility across OIDC-compliant identity providers. [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L8-L11]
- [InformalDecision] `ResultExtensions.ToProblem` provides an extension method on `Result` that standardizes business failure mapping into RFC 9457 Problem Details responses across all HTTP endpoints in the API. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L6-L8]
- [InformalDecision] DomainEventsToOutboxInterceptor inherits from SaveChangesInterceptor to convert domain events buffered on tracked aggregates into OutboxMessage records right before SaveChanges commits, ensuring atomic persistence without dual writes. [src/ClaimFlow.BuildingBlocks/Outbox/DomainEventsToOutboxInterceptor.cs:L1-L13]
- [InformalDecision] OutboxOptions configures outbox processing with default values of a 2-second PollingInterval and a BatchSize of 50 under configuration section name 'Outbox'. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxProcessor.cs:L23-L29]
- [InformalDecision] Resolves outbox event types via an in-memory closed frozen dictionary keyed by type name rather than Type.GetType, preventing arbitrary type instantiation from tampered outbox rows. [src/ClaimFlow.BuildingBlocks/Outbox/OutboxEventRegistry.cs:L13-L15]
- [InformalDecision] Standard OIDC claim names ('sub', 'name', 'role') are kept raw without inbound claim mapping (MapInboundClaims = false) to ensure compatibility across compliant identity providers. [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L8-L11]

## Skills

- `add-api-endpoint`: Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results. (`.github/skills/add-api-endpoint/SKILL.md`)
- `add-domain-event-handler`: Implement and register an outbox domain event handler to process committed domain events. (`.github/skills/add-domain-event-handler/SKILL.md`)
- `configure-module-outbox`: Configure a module DbContext and service collection to persist and dispatch outbox messages for domain events. (`.github/skills/configure-module-outbox/SKILL.md`)
- `map-result-to-problem`: Convert a domain or application failure Result into an RFC 9457 HTTP ProblemDetails response. (`.github/skills/map-result-to-problem/SKILL.md`)
