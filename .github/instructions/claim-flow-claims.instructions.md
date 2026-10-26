---
applyTo: 'src/Modules/Claims/ClaimFlow.Claims/**'
---

# ClaimFlow.Claims

`src/Modules/Claims/ClaimFlow.Claims/` is a dotnet project of this repository, declared by `src/Modules/Claims/ClaimFlow.Claims/ClaimFlow.Claims.csproj`. These instructions add to the repository's own; for files here, where the two differ, these hold.

## Build and test

- `dotnet build src/Modules/Claims/ClaimFlow.Claims/ClaimFlow.Claims.csproj`

## Depends on

Depends on `claim-flow-building-blocks`, `claim-flow-claims-domain` in this repository, as its build file declares. Change those through their own instructions, and run their tests too when a change here needs one there.

## Rules

Each rule cites the code it rests on.

- [Invariant] MapClaimsModule requires both the api and root IEndpointRouteBuilder arguments to be non-null. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L48-L50]
- [Invariant] Policy numbers must be stored and matched in uppercase form. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimDirectory.cs:L17-L18]
- [Invariant] AddClaimsModule requires the provided IHostApplicationBuilder instance to be non-null. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L27-L46]
- [Invariant] The claims table must be completely empty for demo seeding to proceed; if any claim already exists, SeedAsync exits immediately without modifying database state. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsSeeder.cs:L26-L29]
- [BusinessRule] Declaring a claim via the POST endpoint requires the caller to satisfy the ClaimPermissions.WritePolicy authorization policy. [src/Modules/Claims/ClaimFlow.Claims/Features/DeclareClaim.cs:L17-L23]
- [BusinessRule] The delegated approval limit for a claim handler defaults to 10,000 EUR, whereas managers have no limit. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimsOptions.cs:L7-L9]
- [BusinessRule] Settling a claim (ClaimAction.Settle) requires the user to hold the Manager role, whereas any other claim action requires the user to hold at least one Writer role (Handler or Manager). [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L26-L28]
- [BusinessRule] The builder argument passed to AddClaimsModule must not be null; otherwise, an ArgumentNullException is thrown. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L26]
- [BusinessRule] Both api and root arguments passed to MapClaimsModule must not be null; otherwise, an ArgumentNullException is thrown. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L47-L50]
- [BusinessRule] Access under ReadPolicy requires callers to possess roles defined in ClaimPermissions.ReaderRoles, and access under WritePolicy requires roles defined in ClaimPermissions.WriterRoles. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L38-L41]
- [InformalDecision] Design-time DbContext creation uses a hardcoded PostgreSQL connection string ('Host=localhost;Database=claimflow') alongside ClaimsModule.ConfigureNpgsql rather than reading connection configuration from application configuration files. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsDbContextFactory.cs:L9-L12]
- [InformalDecision] ConfigureNpgsql configures the EF Core migrations history table to be stored as '__ef_migrations_history' under the module's dedicated schema (ClaimsDbContext.Schema). [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L78-L79]
- [InformalDecision] Features use ClaimsDbContext directly instead of an abstraction or repository layer, adhering to ADR 0004 since DbContext inherently acts as a repository and unit of work. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsDbContext.cs:L8-L12]
- [InformalDecision] BuildConnectionString parses the connection string named 'claimflow' via NpgsqlConnectionStringBuilder and explicitly disables GSS encryption (GssEncryptionMode.Disable) because Kerberos is not used and the chiseled container runtime lacks libgssapi. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L74-L79]
- [InformalDecision] MapClaimsModule maps the /claims endpoint group with read authorization policy and tags, maps endpoints for DeclareClaim, GetClaimCapabilities, ListClaims, GetClaimStats, GetClaim, and ApplyClaimAction, and maps the ClaimsHub SignalR hub on the root endpoint builder. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L47-L59]
- [InformalDecision] OrderByDescending on DeclaredAt is executed on the Claim entity before projecting into ClaimSnapshot because Entity Framework cannot translate sorting operations on constructed records. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimDirectory.cs:L19-L20]
- [InformalDecision] Claim.Version is marked with IsRowVersion() so Npgsql maps the uint row version onto PostgreSQL's xmin system column, achieving optimistic concurrency without an additional database column. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimConfiguration.cs:L33-L34]
- [InformalDecision] Claim.Type and Claim.Status enums are stored as strings in the database to keep data readable in SQL and protect against enum reordering. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimConfiguration.cs:L29-L31]
- [InformalDecision] ClaimSnapshot exposes claim data as a flat projection of primitive and framework types (Guid, string, DateOnly, DateTimeOffset, decimal) without referencing domain types, decoupling consumers from internal Claims domain models. [src/Modules/Claims/ClaimFlow.Claims/Contracts/IClaimDirectory.cs:L3-L16]
- [InformalDecision] AddClaimsModule registers the Claims module's dependencies, including transactional outbox support for ClaimDeclared and ClaimStatusChanged events, pooled ClaimsDbContext with Npgsql and domain event outbox interceptor, Aspire Npgsql enrichment, options binding, authorization policies for read and write roles, and domain event handlers. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L45]
- [InformalDecision] ClaimsDbContext serves directly as the unit of work and repository for features in the module, intentionally omitting an extra repository abstraction layer. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsDbContext.cs:L7-L11]
- [InformalDecision] Claim numbers are generated by querying the PostgreSQL database sequence directly via raw SQL (SELECT nextval('claims.claim_number_seq')) to guarantee collision-free references across concurrent transactions without locking. [src/Modules/Claims/ClaimFlow.Claims/Persistence/ClaimsDbContext.cs:L19-L26]

## Skills

- `add-api-endpoint`: Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results. (`.github/skills/add-api-endpoint/SKILL.md`)
- `add-domain-event-handler`: Implement and register an outbox domain event handler to process committed domain events. (`.github/skills/add-domain-event-handler/SKILL.md`)
- `register-modular-slice`: Configure and register a modular vertical slice with DI, database pooling, outbox, and endpoint routing. (`.github/skills/register-modular-slice/SKILL.md`)
- `define-module-permissions`: Create strongly typed module configuration options with a standard section name and register them in the module's dependency injection container. (`.github/skills/define-module-permissions/SKILL.md`)
