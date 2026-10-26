---
applyTo: 'src/Modules/Documents/ClaimFlow.Documents/**'
---

# ClaimFlow.Documents

`src/Modules/Documents/ClaimFlow.Documents/` is a dotnet project of this repository, declared by `src/Modules/Documents/ClaimFlow.Documents/ClaimFlow.Documents.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/Modules/Documents/ClaimFlow.Documents/ClaimFlow.Documents.csproj`

## Depends on

Depends on `claim-flow-building-blocks`, `claim-flow-claims`, `claim-flow-documents-domain` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] The claim snapshot and documents list passed to ForClaim must not be null; an ArgumentNullException is thrown if either is null. [src/Modules/Documents/ClaimFlow.Documents/Analysis/AnalysisPrompt.cs:L28-L32]
- [Invariant] api cannot be null when calling MapDocumentsModule; an ArgumentNullException is thrown if it is null. [src/Modules/Documents/ClaimFlow.Documents/DocumentsModule.cs:L66-L68]
- [Invariant] SeedAsync aborts immediately without adding documents or saving files if any document records already exist in the database. [src/Modules/Documents/ClaimFlow.Documents/Seeding/DemoDocumentsSeeder.cs:L14-L24]
- [Invariant] The DocumentIndex field in a CitedPassage refers to the positional index of the corresponding AnalysisDocument in the sequence presented to the analysis model. [src/Modules/Documents/ClaimFlow.Documents/Analysis/AnalysisModels.cs:L3-L4]
- [Invariant] The 'claim-documents' blob container must always be created with private access (PublicAccessType.None), ensuring files are never publicly exposed and are only served via API endpoints after authorization. [src/Modules/Documents/ClaimFlow.Documents/Storage/BlobDocumentStore.cs:L6-L14]
- [BusinessRule] Document authorization enforces two policies: ReadPolicy requires callers to possess DocumentPermissions.Readers roles, and WritePolicy requires callers to possess DocumentPermissions.Writers roles. [src/Modules/Documents/ClaimFlow.Documents/DocumentsModule.cs:L54-L56]
- [BusinessRule] AI analysis is enabled if and only if ApiKey is not null, empty, or consisting only of white-space characters. [src/Modules/Documents/ClaimFlow.Documents/Analysis/AiOptions.cs:L17-L17]
- [BusinessRule] The AI analysis endpoint is rate-limited per user subject ('sub' claim, defaulting to 'anonymous') to a fixed window of at most 6 requests per 1-hour window. [src/Modules/Documents/ClaimFlow.Documents/DocumentsModule.cs:L57-L63]
- [BusinessRule] The maximum total payload byte size for an AI analysis request defaults to 24 MB (24 * 1024 * 1024 bytes) to stay safely under the API's 32 MB limit. [src/Modules/Documents/ClaimFlow.Documents/Analysis/AiOptions.cs:L14-L15]
- [Assumption] ClaimDocument.Upload succeeds and produces a valid result for every hardcoded seeded document payload. [src/Modules/Documents/ClaimFlow.Documents/Seeding/DemoDocumentsSeeder.cs:L44-L53]
- [Assumption] Design-time migrations generation assumes that operations either do not require a live database connection or that a local PostgreSQL server is reachable at 'Host=localhost;Database=claimflow'. [src/Modules/Documents/ClaimFlow.Documents/Persistence/DocumentsDbContextFactory.cs:L10-L12]
- [InformalDecision] AiOptions defaults the AI model identifier to "claude-opus-5-5". [src/Modules/Documents/ClaimFlow.Documents/Analysis/AiOptions.cs:L10-L10]
- [InformalDecision] AiOptions sets the default thinking depth/token spend Effort to "medium". [src/Modules/Documents/ClaimFlow.Documents/Analysis/AiOptions.cs:L12-L13]
- [InformalDecision] ClaimAnalysis stores Citations as an owned collection mapped to a JSON column (ToJson) rather than a separate relational join table, as citations travel with their analysis and are not queried independently. [src/Modules/Documents/ClaimFlow.Documents/Persistence/DocumentsDbContext.cs:L37-L38]
- [InformalDecision] The document analysis models (AnalysisDocument, CitedBlock, CitedPassage, and AnalysisOutcome) are defined as internal sealed C# records to restrict visibility to the Documents module and provide immutable positional representations for intermediate analysis data. [src/Modules/Documents/ClaimFlow.Documents/Analysis/AnalysisModels.cs:L3-L17]
- [InformalDecision] Claim AI reviews are stored as persistent entities rather than regenerated on every view, preserving an immutable audit record of the handler's view at decision time, tracking model and requester metadata, and avoiding repeated model inference costs. [src/Modules/Documents/ClaimFlow.Documents/Analysis/ClaimAnalysis.cs:L3-L7]
- [InformalDecision] Npgsql migrations history table is explicitly placed in the schema designated by DocumentsDbContext.Schema under the name '__ef_migrations_history'. [src/Modules/Documents/ClaimFlow.Documents/DocumentsModule.cs:L97-L98]
- [InformalDecision] BlobDocumentStore implements IDocumentStore using Azure Blob Storage via BlobServiceClient, using a dedicated blob container named 'claim-documents' to persist claim evidence documents. [src/Modules/Documents/ClaimFlow.Documents/Storage/BlobDocumentStore.cs:L7-L11]
- [InformalDecision] ClaimDocument.ClaimId is treated as an unconstrained claim identifier rather than a database foreign key constraint, keeping the claims table isolated within its own module's schema per ADR 0001. [src/Modules/Documents/ClaimFlow.Documents/Persistence/DocumentsDbContext.cs:L21-L21]
- [InformalDecision] A static connection string 'Host=localhost;Database=claimflow' is hardcoded directly into the design-time factory's UseNpgsql options rather than resolving database credentials from configuration files or environment variables. [src/Modules/Documents/ClaimFlow.Documents/Persistence/DocumentsDbContextFactory.cs:L10-L12]
