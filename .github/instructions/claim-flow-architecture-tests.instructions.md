---
applyTo: 'tests/ClaimFlow.ArchitectureTests/**'
---

# ClaimFlow.ArchitectureTests

`tests/ClaimFlow.ArchitectureTests/` is a dotnet project of this repository, declared by `tests/ClaimFlow.ArchitectureTests/ClaimFlow.ArchitectureTests.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build tests/ClaimFlow.ArchitectureTests/ClaimFlow.ArchitectureTests.csproj`

## Depends on

Depends on `claim-flow-api` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] SharedKernel assembly types must not depend on infrastructure namespaces (Microsoft.EntityFrameworkCore, Microsoft.AspNetCore, Npgsql, ClaimFlow.BuildingBlocks) [tests/ClaimFlow.ArchitectureTests/ModuleBoundaryTests.cs:L35-L37]
- [InformalDecision] The ClaimFlow.ArchitectureTests project is configured as a .NET project using the Microsoft.NET.Sdk SDK. [tests/ClaimFlow.ArchitectureTests/ClaimFlow.ArchitectureTests.csproj:L1-L1]
- [InformalDecision] The architecture test suite references ClaimFlow.Api (..\..\src\ClaimFlow.Api\ClaimFlow.Api.csproj) as a project dependency to validate architectural constraints on the API assembly. [tests/ClaimFlow.ArchitectureTests/ClaimFlow.ArchitectureTests.csproj:L6-L6]
- [InformalDecision] The architecture test project includes NetArchTest.Rules as a package reference to support fluent architectural rule definitions and verification. [tests/ClaimFlow.ArchitectureTests/ClaimFlow.ArchitectureTests.csproj:L3-L3]
