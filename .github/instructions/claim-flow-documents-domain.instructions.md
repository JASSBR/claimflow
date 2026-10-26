---
applyTo: 'src/Modules/Documents/ClaimFlow.Documents.Domain/**'
---

# ClaimFlow.Documents.Domain

`src/Modules/Documents/ClaimFlow.Documents.Domain/` is a dotnet project of this repository, declared by `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj`

## Depends on

Depends on `claim-flow-shared-kernel` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] The StorageKey property is derived strictly from ClaimId and Id formatted as '{ClaimId:N}/{Id.Value:N}', preventing user-supplied file names from steering storage paths. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L34-L35]
- [BusinessRule] An uploaded document must have a recognized format; if detectedFormat is null, document upload is rejected with DocumentErrors.UnsupportedFormat. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L65-L68]
- [BusinessRule] A claim may have at most 12 attached documents; attempting to upload when documentsAlreadyOnClaim is 12 or greater fails with DocumentErrors.TooManyDocuments. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L14-L14]
- [BusinessRule] A document's size must be strictly greater than 0 bytes and at most 10 MB (10 * 1024 * 1024 bytes); otherwise, errors DocumentErrors.Empty or DocumentErrors.TooLarge are returned. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L55-L63]
- [InformalDecision] DocumentId is implemented as a strongly-typed readonly record struct wrapping a Guid Value. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentId.cs:L3-L3]
- [InformalDecision] Document format identification relies on inspecting the file header leading bytes ("magic numbers") via ReadOnlySpan<byte> rather than trusting browser-supplied Content-Type headers or file extensions. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentFormat.cs:L3-L6]

## Skills

- `define-domain-errors`: Declare structured domain error definitions for an aggregate or module as static readonly Error members using factory methods on Error. (`.github/skills/define-domain-errors/SKILL.md`)
- `add-domain-aggregate`: Implement a domain aggregate root inheriting from AggregateRoot<TId> with a private persistence constructor, factory method returning Result<T>, and static domain error definitions. (`.github/skills/add-domain-aggregate/SKILL.md`)
