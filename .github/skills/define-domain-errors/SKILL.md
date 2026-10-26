---
name: define-domain-errors
description: "Declare structured domain error definitions for an aggregate or module as static readonly Error members using factory methods on Error."
---

> Evidence: value gate verified the facts below against this repository.

Declare structured domain error definitions for an aggregate or module as static readonly Error members using factory methods on Error.

Applies in: `claim-flow-documents-domain`.

## What is specific to this repository

- DocumentErrors defines domain errors as public static readonly Error fields using Error.Validation, Error.Conflict, and Error.NotFound with dot-notated error codes prefixed with 'document.'. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L5-L8]
- Error descriptions interpolate ClaimDocument aggregate constants such as MaxSizeBytes and MaxDocumentsPerClaim. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L10-L11]
- TooManyDocuments defines a conflict error when the count of documents on a claim exceeds MaxDocumentsPerClaim. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L16-L17]
- ClaimDocument defines constants for maximum file size, maximum documents per claim, and maximum file name length. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L12-L14]

## Rules this change touches

- [Invariant] The StorageKey property is derived strictly from ClaimId and Id formatted as '{ClaimId:N}/{Id.Value:N}', preventing user-supplied file names from steering storage paths. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L34-L35)
- [BusinessRule] An uploaded document must have a recognized format; if detectedFormat is null, document upload is rejected with DocumentErrors.UnsupportedFormat. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L65-L68)
- [BusinessRule] A claim may have at most 12 attached documents; attempting to upload when documentsAlreadyOnClaim is 12 or greater fails with DocumentErrors.TooManyDocuments. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L14-L14)
- [BusinessRule] A document's size must be strictly greater than 0 bytes and at most 10 MB (10 * 1024 * 1024 bytes); otherwise, errors DocumentErrors.Empty or DocumentErrors.TooLarge are returned. (approved, conviction 1.00, src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L55-L63)

## Context

- The private parameterless constructor ClaimDocument() exists as a materialization constructor for the persistence layer. [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L16-L19]

## Start here

`src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L2-L24] is the worked example for this change, as it stands in the repository:

```csharp

namespace ClaimFlow.Documents.Domain;

public static class DocumentErrors
{
    public static readonly Error UnsupportedFormat =
        Error.Validation("document.unsupported_format", "Only PDF, PNG and JPEG files are accepted (checked from the file content, not its name).");

    public static readonly Error TooLarge =
        Error.Validation("document.too_large", $"A document cannot exceed {ClaimDocument.MaxSizeBytes / (1024 * 1024)} MB.");

    public static readonly Error Empty =
        Error.Validation("document.empty", "The file is empty.");

    public static readonly Error TooManyDocuments =
        Error.Conflict("document.too_many", $"A claim file holds at most {ClaimDocument.MaxDocumentsPerClaim} documents.");

    public static readonly Error ClaimNotFound =
        Error.NotFound("document.claim_not_found", "No claim exists with this id.");

    public static readonly Error NotFound =
        Error.NotFound("document.not_found", "No document exists with this id on this claim.");
}
```

## The same change elsewhere in this repository

- `src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs` [src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimDocument.cs:L56-L56]: `            return DocumentErrors.TooManyDocuments;`

## Patterns

Required:
- `Error.Conflict`
- `Error.NotFound`
- `Error.Validation`
- `public static class`
- `public static readonly Error`
Forbidden:
- Avoid `throw new` in this change.
- Avoid `throw new Exception` in this change.

## Workflow

1. Create a public static class named <Concept>Errors in the Domain namespace. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L3-L3]
2. Define public static readonly Error fields for each specific failure mode. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L7-L7]
3. Use Error.Validation for invalid formats, sizes, or inputs with a dot-notated code. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L8-L8]
4. Use Error.Conflict for business rule or invariant limit violations. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L17-L17]
5. Use Error.NotFound when an entity or parent resource is missing. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L20-L20]

## Checklist

- [ ] Error codes follow lowercase dot-delimited format (e.g. document.too_large). [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L11-L11]
- [ ] Use Error.Validation, Error.Conflict, or Error.NotFound factory methods. [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L8-L8]
- [ ] Error instances are public static readonly Error fields [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L7-L7]
- [ ] Error codes follow hierarchical dot notation like 'document.too_many' [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L17-L17]
- [ ] Use Error.Validation for invalid inputs or bad formats [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L8-L8]
- [ ] Use Error.Conflict for domain state or quota conflicts [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L17-L17]
- [ ] Use Error.NotFound when entities or parent aggregates cannot be located [src/Modules/Documents/ClaimFlow.Documents.Domain/DocumentErrors.cs:L20-L20]
