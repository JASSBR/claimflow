---
applyTo: 'tests/ClaimFlow.IntegrationTests/**'
---

# ClaimFlow.IntegrationTests

`tests/ClaimFlow.IntegrationTests/` is a dotnet project of this repository, declared by `tests/ClaimFlow.IntegrationTests/ClaimFlow.IntegrationTests.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build tests/ClaimFlow.IntegrationTests/ClaimFlow.IntegrationTests.csproj`

## Depends on

Depends on `claim-flow-api` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] An outbox message is considered pending delivery if and only if ProcessedAt is null and Attempts is strictly less than OutboxMessage.MaxAttempts [tests/ClaimFlow.IntegrationTests/ClaimFlowApiFactory.cs:L70-L73]
- [Invariant] A claim document cannot be accessed using a claim ID other than the claim under which the document was uploaded, returning 404 Not Found. [tests/ClaimFlow.IntegrationTests/DocumentsTests.cs:L70-L80]
- [Invariant] When no AI API key is configured, document settings report AiEnabled as false and document analysis requests return 503 Service Unavailable. [tests/ClaimFlow.IntegrationTests/DocumentsTests.cs:L119-L130]
- [Invariant] Unauthenticated requests to API claim endpoints and SignalR hubs must be rejected with HTTP 401 Unauthorized. [tests/ClaimFlow.IntegrationTests/SecurityTests.cs:L17-L24]
- [Invariant] Claim state changes (declaration and workflow actions like StartReview) write outbox records that remain pending until the outbox is drained [tests/ClaimFlow.IntegrationTests/OutboxTests.cs:L24-L34]
- [Invariant] When a claim is declared, it is created with status Declared, a positive version number, and StartReview as its only initial allowed action. [tests/ClaimFlow.IntegrationTests/ClaimsApiTests.cs:L28-L42]
- [BusinessRule] Uploaded claim documents exceeding ClaimDocument.MaxSizeBytes are rejected with Bad Request. [tests/ClaimFlow.IntegrationTests/DocumentsTests.cs:L46-L55]
- [BusinessRule] Uploaded documents must match their claimed format and not be executable binaries masked with a .pdf extension; files failing format inspection are rejected with Bad Request and document.unsupported_format error. [tests/ClaimFlow.IntegrationTests/DocumentsTests.cs:L35-L44]
- [BusinessRule] Users with an Auditor role can view claim documents but are forbidden from uploading new documents or triggering document analysis on claims. [tests/ClaimFlow.IntegrationTests/DocumentsTests.cs:L57-L68]
- [InformalDecision] Integration tests rely on Testcontainers with Testcontainers.Azurite and Testcontainers.PostgreSql packages to spin up containerized storage and database dependencies. [tests/ClaimFlow.IntegrationTests/ClaimFlow.IntegrationTests.csproj:L5-L6]
- [InformalDecision] The integration test project incorporates Microsoft.AspNetCore.Mvc.Testing for hosting in-memory test servers and Microsoft.AspNetCore.SignalR.Client for testing SignalR real-time hubs. [tests/ClaimFlow.IntegrationTests/ClaimFlow.IntegrationTests.csproj:L3-L4]
