---
applyTo: 'src/ClaimFlow.AppHost/**'
---

# ClaimFlow.AppHost

`src/ClaimFlow.AppHost/` is a dotnet project of this repository, declared by `src/ClaimFlow.AppHost/ClaimFlow.AppHost.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/ClaimFlow.AppHost/ClaimFlow.AppHost.csproj`

## Depends on

Depends on `claim-flow-api` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [InformalDecision] ClaimFlow.AppHost configures development logging to capture Information level messages by default, while suppressing Microsoft.AspNetCore framework logs below Warning level. [src/ClaimFlow.AppHost/appsettings.Development.json:L1-L8]
