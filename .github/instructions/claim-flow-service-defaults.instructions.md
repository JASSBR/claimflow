---
applyTo: 'src/ClaimFlow.ServiceDefaults/**'
---

# ClaimFlow.ServiceDefaults

`src/ClaimFlow.ServiceDefaults/` is a dotnet project of this repository, declared by `src/ClaimFlow.ServiceDefaults/ClaimFlow.ServiceDefaults.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/ClaimFlow.ServiceDefaults/ClaimFlow.ServiceDefaults.csproj`

## Rules

Each rule cites the code it rests on.

- [InformalDecision] The project configures IsAspireSharedProject to true, designating the project as a shared .NET Aspire service defaults library. [src/ClaimFlow.ServiceDefaults/ClaimFlow.ServiceDefaults.csproj:L3-L5]
