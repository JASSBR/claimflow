---
name: register-modular-slice
description: "Configure and register a modular vertical slice with DI, database pooling, outbox, and endpoint routing."
---

> Evidence: value gate verified the facts below against this repository.

Configure and register a modular vertical slice with DI, database pooling, outbox, and endpoint routing.

Applies in: `claim-flow-claims`.

## What is specific to this repository

- The monolith database connection string is named "claimflow". [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L24]
- EF Core migrations history is configured in Npgsql to use the "__ef_migrations_history" table in the module schema. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L88-L88]
- GSS encryption is disabled in Npgsql connection strings because the chiseled runtime image lacks libgssapi and Kerberos is unused. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L84-L84]

## Rules this change touches

- [Invariant] MapClaimsModule requires both the api and root IEndpointRouteBuilder arguments to be non-null. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L48-L50)
- [Invariant] AddClaimsModule requires the provided IHostApplicationBuilder instance to be non-null. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L27-L46)
- [BusinessRule] The builder argument passed to AddClaimsModule must not be null; otherwise, an ArgumentNullException is thrown. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L26)
- [BusinessRule] Both api and root arguments passed to MapClaimsModule must not be null; otherwise, an ArgumentNullException is thrown. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L47-L50)
- [BusinessRule] Access under ReadPolicy requires callers to possess roles defined in ClaimPermissions.ReaderRoles, and access under WritePolicy requires roles defined in ClaimPermissions.WriterRoles. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L38-L41)
- [InformalDecision] ConfigureNpgsql configures the EF Core migrations history table to be stored as '__ef_migrations_history' under the module's dedicated schema (ClaimsDbContext.Schema). (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L78-L79)
- [InformalDecision] BuildConnectionString parses the connection string named 'claimflow' via NpgsqlConnectionStringBuilder and explicitly disables GSS encryption (GssEncryptionMode.Disable) because Kerberos is not used and the chiseled container runtime lacks libgssapi. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L74-L79)
- [InformalDecision] MapClaimsModule maps the /claims endpoint group with read authorization policy and tags, maps endpoints for DeclareClaim, GetClaimCapabilities, ListClaims, GetClaimStats, GetClaim, and ApplyClaimAction, and maps the ClaimsHub SignalR hub on the root endpoint builder. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L47-L59)
- [InformalDecision] AddClaimsModule registers the Claims module's dependencies, including transactional outbox support for ClaimDeclared and ClaimStatusChanged events, pooled ClaimsDbContext with Npgsql and domain event outbox interceptor, Aspire Npgsql enrichment, options binding, authorization policies for read and write roles, and domain event handlers. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L45)

## Context

- BuildConnectionString retrieves the connection string named 'claimflow' from configuration and sets GssEncryptionMode to Disable on NpgsqlConnectionStringBuilder because the chiseled runtime image lacks libgssapi. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L72-L76]
- ClaimsModule serves as the single public entry point for the Claims module in the host application, exposing registration, endpoint mapping, and database initialization extension methods. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L19-L20]

## Start here

`src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L23-L62] is the worked example for this change, as it stands in the repository:

```csharp
    /// <summary>One database for the whole monolith; each module owns a schema in it (ADR 0001).</summary>
    public const string ConnectionStringName = "claimflow";

    public static IHostApplicationBuilder AddClaimsModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOutbox<ClaimsDbContext>(builder.Configuration, typeof(ClaimDeclared), typeof(ClaimStatusChanged));
        builder.Services.AddDbContextPool<ClaimsDbContext>((provider, options) => options
            .UseNpgsql(BuildConnectionString(provider.GetRequiredService<IConfiguration>()), ConfigureNpgsql)
            .AddInterceptors(provider.GetRequiredService<DomainEventsToOutboxInterceptor>()));
        // Adds Aspire's retries, health check and OpenTelemetry instrumentation to the context registered above.
        builder.EnrichNpgsqlDbContext<ClaimsDbContext>();

        builder.Services.AddOptions<ClaimsOptions>().Bind(builder.Configuration.GetSection(ClaimsOptions.SectionName));
        builder.Services.AddSingleton<ClaimPermissions>();
        builder.Services.AddScoped<IClaimDirectory, ClaimDirectory>();
        // The module declares its own policies: the host only knows that endpoints require *some* authorization.
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(ClaimPermissions.ReadPolicy, policy => policy.RequireRole(ClaimPermissions.ReaderRoles))
            .AddPolicy(ClaimPermissions.WritePolicy, policy => policy.RequireRole(ClaimPermissions.WriterRoles));

        builder.Services.AddScoped<IDomainEventHandler<ClaimDeclared>, ClaimChangedNotifier>();
        builder.Services.AddScoped<IDomainEventHandler<ClaimStatusChanged>, ClaimChangedNotifier>();
        return builder;
    }

    public static IEndpointRouteBuilder MapClaimsModule(this IEndpointRouteBuilder api, IEndpointRouteBuilder root)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentNullException.ThrowIfNull(root);

        var claims = api.MapGroup("/claims").WithTags("Claims").RequireAuthorization(ClaimPermissions.ReadPolicy);
        DeclareClaim.Map(claims);
        GetClaimCapabilities.Map(claims);
        ListClaims.Map(claims);
        GetClaimStats.Map(claims);
        GetClaim.Map(claims);
        ApplyClaimAction.Map(claims);

```

## Patterns

Required:
- `AddDbContextPool`
- `AddOutbox`
- `EnrichNpgsqlDbContext`
- `IEndpointRouteBuilder`
- `IHostApplicationBuilder`
Forbidden:
- Avoid `UseEndpoints` in this change.

## Workflow

1. Create a static module entry class declaring Add{ModuleName}Module on IHostApplicationBuilder and Map{ModuleName}Module on IEndpointRouteBuilder [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L26-L26]
2. Register transactional outbox using AddOutbox with relevant domain event types [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L30-L30]
3. Register EF Core DbContext with AddDbContextPool and attach DomainEventsToOutboxInterceptor [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L31-L31]
4. Call EnrichNpgsqlDbContext to enable Aspire instrumentation, resilience, and health checks [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L35-L35]
5. Define module-specific authorization policies using AddAuthorizationBuilder [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L41-L41]
6. Map feature endpoints under a tagged route group via MapGroup on IEndpointRouteBuilder [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L55-L55]

## Checklist

- [ ] Verify the module exposes an extension method Add{ModuleName}Module on IHostApplicationBuilder [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L26-L26]
- [ ] Ensure Map{ModuleName}Module maps routes to an IEndpointRouteBuilder with a top-level route group and tag [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L50-L50]
- [ ] Register the DbContext using AddDbContextPool and enrich it via EnrichNpgsqlDbContext [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L31-L31]
- [ ] Register the transactional outbox with AddOutbox for published domain events [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L30-L30]
