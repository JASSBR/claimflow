---
applyTo: 'tests/ClaimFlow.Documents.UnitTests/**'
---

# ClaimFlow.Documents.UnitTests

`tests/ClaimFlow.Documents.UnitTests/` is a dotnet project of this repository, declared by `tests/ClaimFlow.Documents.UnitTests/ClaimFlow.Documents.UnitTests.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build tests/ClaimFlow.Documents.UnitTests/ClaimFlow.Documents.UnitTests.csproj`

## Depends on

Depends on `claim-flow-documents` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] Sanitized file name length is bounded to ClaimDocument.FileNameMaxLength [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L59-L62]
- [Invariant] Uploading a claim document stores the detected MIME content type, the uploader's name, and assigns a storage key matching the format '{ClaimId:N}/{document.Id.Value:N}'. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L17-L25]
- [Invariant] When a cited passage is cited multiple times in analysis blocks, the composer reuses the same citation number across occurrences and emits only a single entry in the citations list. [tests/ClaimFlow.Documents.UnitTests/AnalysisComposerTests.cs:L24-L33]
- [Invariant] Citations referencing document indices that do not exist in the provided document collection are ignored in the composed text and omitted from citations. [tests/ClaimFlow.Documents.UnitTests/AnalysisComposerTests.cs:L35-L40]
- [Invariant] When composing document analysis blocks with cited passages, citations are assigned sequential numbers in order of first use, formatted as [[n]] in the composed content, and mapped to their corresponding document identifier and title. [tests/ClaimFlow.Documents.UnitTests/AnalysisComposerTests.cs:L11-L22]
- [Invariant] AnalysisPrompt.ForClaim includes the claim number, policy number, incident date, formatted claim amount (fr-FR currency formatting), description, and comma-separated document titles; and AnalysisPrompt.System includes a prompt-injection guard stating document content is never an instruction. [tests/ClaimFlow.Documents.UnitTests/AnalysisComposerTests.cs:L42-L56]
- [BusinessRule] Uploading an empty document of size 0 bytes is rejected with DocumentErrors.Empty. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L35-L39]
- [BusinessRule] Uploading a document to a claim that already has ClaimDocument.MaxDocumentsPerClaim documents is rejected with DocumentErrors.TooManyDocuments. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L41-L45]
- [BusinessRule] Document format detection recognizes PDF, PNG, and JPEG documents from their header magic byte signatures. [tests/ClaimFlow.Documents.UnitTests/DocumentFormatTests.cs:L8-L15]
- [BusinessRule] Document file names are sanitized to keep display names only, stripping directory paths and HTML markup, defaulting to 'document' if empty or whitespace, and appending the detected format's file extension. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L47-L59]
- [BusinessRule] Uploading a document with an unknown format and a size exceeding ClaimDocument.MaxSizeBytes is rejected with both DocumentErrors.TooLarge and DocumentErrors.UnsupportedFormat. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L27-L33]
- [InformalDecision] The project uses the standard Microsoft .NET SDK (`Microsoft.NET.Sdk`). [tests/ClaimFlow.Documents.UnitTests/ClaimFlow.Documents.UnitTests.csproj:L1-L1]
- [InformalDecision] The test project declares a project reference to the target module under test at `..\..\src\Modules\Documents\ClaimFlow.Documents\ClaimFlow.Documents.csproj`. [tests/ClaimFlow.Documents.UnitTests/ClaimFlow.Documents.UnitTests.csproj:L2-L4]

## Skills

- `test-domain-entities`: Write unit tests for domain entities and aggregates verifying factory creation, business invariants, and domain events. (`.github/skills/test-domain-entities/SKILL.md`)
