---
name: add-domain-aggregate
description: "Implement a domain aggregate root inheriting from AggregateRoot<TId> with a private persistence constructor, factory method returning Result<T>, and static domain error definitions."
---

> Evidence: value gate verified the facts below against this repository.

Implement a domain aggregate root inheriting from AggregateRoot<TId> with a private persistence constructor, factory method returning Result<T>, and static domain error definitions.

Applies in: `claim-flow-documents-domain`.

## What is specific to this repository

- The Documents domain model depends on SharedKernel only, which is enforced by architecture tests. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj:L2-L2]
- ClaimDocument is a sealed aggregate root that inherits from AggregateRoot parameterized with the DocumentId strongly-typed identifier. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L10-L10]
- ClaimDocument defines domain limit constants such as MaxDocumentsPerClaim set to 12. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L13-L13]
- ClaimDocument uses a private parameterless constructor for persistence materialization. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L18-L18]
- ClaimDocument exposes a static Upload factory method returning Result<ClaimDocument>. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L41-L41]

## Rules this change touches

- [Invariant] The StorageKey property is derived strictly from ClaimId and Id formatted as '{ClaimId:N}/{Id.Value:N}', preventing user-supplied file names from steering storage paths. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L34-L35)
- [BusinessRule] An uploaded document must have a recognized format; if detectedFormat is null, document upload is rejected with DocumentErrors.UnsupportedFormat. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L65-L68)
- [BusinessRule] A claim may have at most 12 attached documents; attempting to upload when documentsAlreadyOnClaim is 12 or greater fails with DocumentErrors.TooManyDocuments. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L14-L14)
- [BusinessRule] A document's size must be strictly greater than 0 bytes and at most 10 MB (10 * 1024 * 1024 bytes); otherwise, errors DocumentErrors.Empty or DocumentErrors.TooLarge are returned. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L55-L63)

## Context

- The private parameterless constructor ClaimDocument() exists as a materialization constructor for the persistence layer. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L16-L19]

## Start here

`src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj:L1-L6] is the worked example for this change, as it stands in the repository:

```
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Documents domain model: depends on SharedKernel only (enforced by ArchitectureTests). -->
  <ItemGroup>
    <ProjectReference Include="..\..\..\ClaimFlow.SharedKernel\ClaimFlow.SharedKernel.csproj" />
  </ItemGroup>
</Project>
```

## The same change elsewhere in this repository

- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L10-L10]: `public sealed class ClaimDocument : AggregateRoot<DocumentId>`
- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L16-L16]: `    private ClaimDocument()`
- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L18-L18]: `// Materialization constructor for the persistence layer.`
- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L41-L41]: `public static Result<ClaimDocument> Upload(`
- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L54-L54]: `        if (documentsAlreadyOnClaim >= MaxDocumentsPerClaim)`
- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L56-L56]: `return DocumentErrors.TooManyDocuments;`
- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L76-L76]: `return Result.Failure<ClaimDocument>([.. errors]);`

## Patterns

Required:
- `AggregateRoot<`
- `Result.Failure<`
- `Result<`
- `private set`
Forbidden:
- Avoid `public ClaimDocument(` in this change.
- Avoid `throw new Exception` in this change.

## Workflow

1. Ensure the domain project references only ClaimFlow.SharedKernel. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj:L4-L4]
2. Inherit the aggregate class from AggregateRoot<TId> using a strongly-typed identifier. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L10-L10]
3. Implement a static factory method returning Result<TAggregate> that validates business rules against domain errors. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L41-L41]

## Checklist

- [ ] Does the aggregate root inherit from AggregateRoot<TId>? [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L10-L10]
- [ ] Does the factory method return Result<TAggregate> instead of throwing domain exceptions? [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L41-L41]
- [ ] Are domain errors declared using Error.Validation, Error.Conflict, or Error.NotFound? [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L59-L59]
- [ ] Does the domain project depend solely on ClaimFlow.SharedKernel? [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj:L4-L4]
- [ ] Validate inputs in a static factory method and return Result.Failure with domain errors on validation failure [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L76-L76]
- [ ] Aggregate inherits AggregateRoot<TId> from ClaimFlow.SharedKernel [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L10-L10]
- [ ] Static constants define limits (e.g. MaxSizeBytes, MaxDocumentsPerClaim) [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L12-L12]
