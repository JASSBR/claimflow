---
applyTo: 'src/ClaimFlow.Api/**'
---

# ClaimFlow.Api

`src/ClaimFlow.Api/` is a dotnet project of this repository, declared by `src/ClaimFlow.Api/ClaimFlow.Api.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/ClaimFlow.Api/ClaimFlow.Api.csproj`

## Depends on

Depends on `claim-flow-claims`, `claim-flow-documents`, `claim-flow-service-defaults` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] Unhandled exceptions are masked from the client: ProblemDetails responses only return static titles ('The request is malformed.' for BadHttpRequestException, or 'An unexpected error occurred.' otherwise) and the corresponding status code without echoing raw exception messages or stack traces to callers regardless of the environment. [src/ClaimFlow.Api/UnhandledExceptionHandler.cs:L5-L38]
- [BusinessRule] When AuthMode is Oidc, Authority must not be null or whitespace; otherwise validation fails with 'Auth:Authority is required when Auth:Mode is Oidc.' [src/ClaimFlow.Api/Security/AuthOptions.cs:L32-L35]
- [BusinessRule] Enum values in JSON payloads for both HTTP APIs and SignalR connections must be represented exclusively by their name strings; numeric integer values are rejected during deserialization. [src/ClaimFlow.Api/Program.cs:L18-L20]
- [BusinessRule] When AuthMode is Demo, DemoSigningKey must not be null and must be at least 32 characters long; otherwise validation fails with 'Auth:DemoSigningKey (≥ 32 chars) is required when Auth:Mode is Demo.' [src/ClaimFlow.Api/Security/AuthOptions.cs:L37-L40]
- [Assumption] Scalar's API reference UI is hosted under path prefix '/scalar' and requires external or inline scripts and styles that would fail to load under a strict 'default-src \'none\'' Content-Security-Policy. [src/ClaimFlow.Api/SecurityHeaders.cs:L13-L17]
- [InformalDecision] The security headers middleware sets X-Content-Type-Options to 'nosniff', X-Frame-Options to 'DENY', and Referrer-Policy to 'no-referrer' on all outgoing HTTP responses. [src/ClaimFlow.Api/SecurityHeaders.cs:L9-L12]
- [InformalDecision] Application logging is configured with a default level of 'Information', while 'Microsoft.AspNetCore' and 'Microsoft.EntityFrameworkCore.Database.Command' log levels are suppressed to 'Warning'. [src/ClaimFlow.Api/appsettings.json:L3-L7]
- [InformalDecision] A strict Content-Security-Policy header ('default-src \'none\'; frame-ancestors \'none\'') is attached to all HTTP responses unless the request path begins with '/scalar' (case-insensitive). [src/ClaimFlow.Api/SecurityHeaders.cs:L13-L17]
- [InformalDecision] Define rate limiting policy names as public constant strings on an internal static class RateLimiting to prevent string duplication across rate limiting configuration and endpoint mapping. [src/ClaimFlow.Api/RateLimiting.cs:L3-L6]

## Skills

- `add-api-endpoint`: Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results. (`.github/skills/add-api-endpoint/SKILL.md`)
