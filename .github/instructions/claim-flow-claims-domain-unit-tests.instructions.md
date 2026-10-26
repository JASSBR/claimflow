---
applyTo: 'tests/ClaimFlow.Claims.Domain.UnitTests/**'
---

# ClaimFlow.Claims.Domain.UnitTests

`tests/ClaimFlow.Claims.Domain.UnitTests/` is a dotnet project of this repository, declared by `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimFlow.Claims.Domain.UnitTests.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build tests/ClaimFlow.Claims.Domain.UnitTests/ClaimFlow.Claims.Domain.UnitTests.csproj`

## Depends on

Depends on `claim-flow-claims-domain` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] Implicit conversion of a value to a Result<T> yields a successful Result carrying that value. [tests/ClaimFlow.Claims.Domain.UnitTests/ResultTests.cs:L18-L24]
- [Invariant] A newly declared claim successfully transitions to under review when review is initiated by the assigned handler. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimBuilder.cs:L26-L31]
- [Invariant] A successfully declared claim is created with status Declared, empty history, and StartReview as its only allowed action. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L8-L21]
- [Invariant] Creating a failed Result requires at least one error; invoking Result.Failure without errors throws an ArgumentException. [tests/ClaimFlow.Claims.Domain.UnitTests/ResultTests.cs:L26-L30]
- [Invariant] Accessing the Value of a failed Result throws an InvalidOperationException. [tests/ClaimFlow.Claims.Domain.UnitTests/ResultTests.cs:L10-L16]
- [BusinessRule] A claim's allowed actions are governed by its status: Declared permits StartReview; UnderReview permits RequestInformation, Approve, and Reject; InformationRequested permits ResumeReview and Reject; and Approved permits Settle. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143]
- [InformalDecision] ClaimFlow.Claims.Domain.UnitTests is configured as an MSBuild project targeting the Microsoft.NET.Sdk SDK. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimFlow.Claims.Domain.UnitTests.csproj:L1-L1]
- [BusinessRule] UnderReview, so it follows the normal path again: request information, approve or reject. (not yet reviewed) [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143]

## Skills

- `test-domain-entities`: Write unit tests for domain entities and aggregates verifying factory creation, business invariants, and domain events. (`.github/skills/test-domain-entities/SKILL.md`)
