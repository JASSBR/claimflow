---
applyTo: 'src/ClaimFlow.SharedKernel/**'
---

# ClaimFlow.SharedKernel

`src/ClaimFlow.SharedKernel/` is a dotnet project of this repository, declared by `src/ClaimFlow.SharedKernel/ClaimFlow.SharedKernel.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/ClaimFlow.SharedKernel/ClaimFlow.SharedKernel.csproj`

## Rules

Each rule cites the code it rests on.

- [Invariant] The internal _domainEvents list is encapsulated and exposed externally only as a read-only collection via DomainEvents.AsReadOnly(), preventing external mutation without going through Raise or ClearDomainEvents. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L21-L24]
- [InformalDecision] ClaimFlow.SharedKernel uses the standard 'Microsoft.NET.Sdk' project SDK without external package or project references declared in its project file. [src/ClaimFlow.SharedKernel/ClaimFlow.SharedKernel.csproj:L1-L1]
- [InformalDecision] Expected business failures are modeled as returned values via Result and Result<T> to allow returning multiple errors simultaneously, reserving exceptions solely for unexpected runtime situations. [src/ClaimFlow.SharedKernel/Result.cs:L3-L6]

## Skills

- `add-aggregate-root`: Define a domain aggregate root inheriting from AggregateRoot<TId> that buffers domain events for outbox persistence. (`.github/skills/add-aggregate-root/SKILL.md`)
- `register-domain-event`: Define a domain event implementing IDomainEvent with an occurrence timestamp for aggregate state changes. (`.github/skills/register-domain-event/SKILL.md`)
