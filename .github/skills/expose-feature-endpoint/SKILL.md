---
name: expose-feature-endpoint
description: "Add an HTTP feature endpoint mapping and handler using ClaimsDbContext and expose vertical slice features via Map methods."
---

> Evidence: value gate verified the facts below against this repository.

Add an HTTP feature endpoint mapping and handler using ClaimsDbContext and expose vertical slice features via Map methods.

Applies in: `claim-flow-api`, `claim-flow-claims`.

## What is specific to this repository

- The Claims module groups its HTTP endpoints under "/claims" with the "Claims" tag and requires ClaimPermissions.ReadPolicy authorization. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L55-L55]
- The Claims module registers ReadPolicy and WritePolicy authorization policies based on ClaimPermissions reader and writer roles. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L41-L43]
- Claims features from the Features namespace are exposed on the claims route group via their static Map methods. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L56-L61]

## Rules this change touches

- [BusinessRule] The builder argument passed to AddClaimsModule must not be null; otherwise, an ArgumentNullException is thrown. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L26)
- [BusinessRule] Both api and root arguments passed to MapClaimsModule must not be null; otherwise, an ArgumentNullException is thrown. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L47-L50)
- [InformalDecision] ConfigureNpgsql configures the EF Core migrations history table to be stored as '__ef_migrations_history' under the module's dedicated schema (ClaimsDbContext.Schema). (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L78-L79)
- [InformalDecision] MapClaimsModule maps the /claims endpoint group with read authorization policy and tags, maps endpoints for DeclareClaim, GetClaimCapabilities, ListClaims, GetClaimStats, GetClaim, and ApplyClaimAction, and maps the ClaimsHub SignalR hub on the root endpoint builder. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L47-L59)
- [InformalDecision] AddClaimsModule registers the Claims module's dependencies, including transactional outbox support for ClaimDeclared and ClaimStatusChanged events, pooled ClaimsDbContext with Npgsql and domain event outbox interceptor, Aspire Npgsql enrichment, options binding, authorization policies for read and write roles, and domain event handlers. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L24-L45)

## Context

- BuildConnectionString retrieves the connection string named 'claimflow' from configuration and sets GssEncryptionMode to Disable on NpgsqlConnectionStringBuilder because the chiseled runtime image lacks libgssapi. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L72-L76]
- ClaimsModule serves as the single public entry point for the Claims module in the host application, exposing registration, endpoint mapping, and database initialization extension methods. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L19-L20]

## Start here

`src/ClaimFlow.Api/Security/DemoIdentityProvider.cs` [src/ClaimFlow.Api/Security/DemoIdentityProvider.cs:L32-L71] is the worked example for this change, as it stands in the repository:

```csharp
            "Consulte tout, ne modifie rien.", [ClaimsRoles.Auditor]),
    ];

    public static IEndpointRouteBuilder MapDemoIdentityProvider(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth").WithTags("Demo identity provider").AllowAnonymous();
        auth.MapGet("/personas", () => TypedResults.Ok(Personas))
            .WithSummary("Demo personas available on the login screen");
        auth.MapPost("/token", IssueToken)
            .WithSummary("Issue a demo access token for a persona");
        return endpoints;
    }

    private static Results<Ok<DemoTokenResponse>, NotFound> IssueToken(
        DemoTokenRequest request,
        IOptions<AuthOptions> options,
        TimeProvider timeProvider)
    {
        var persona = Personas.FirstOrDefault(p => string.Equals(p.Id, request.PersonaId, StringComparison.Ordinal));
        if (persona is null)
        {
            return TypedResults.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.Add(options.Value.DemoTokenLifetime);
        var claims = new List<Claim>
        {
            new(ClaimsPrincipalExtensions.SubjectClaim, persona.Id),
            new(ClaimsPrincipalExtensions.NameClaim, persona.Name),
            new("title", persona.Title),
        };
        claims.AddRange(persona.Roles.Select(role => new Claim(ClaimsPrincipalExtensions.RoleClaim, role)));

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = AuthOptions.DemoIssuer,
            Audience = options.Value.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
```

## The same change elsewhere in this repository

- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L55-L55]: `var claims = api.MapGroup("/claims").WithTags("Claims").RequireAuthorization(ClaimPermissions.ReadPolicy);`
- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L56-L56]: `DeclareClaim.Map(claims);`
- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L57-L57]: `GetClaimCapabilities.Map(claims);`
- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L58-L58]: `ListClaims.Map(claims);`
- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L59-L59]: `GetClaimStats.Map(claims);`
- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L60-L60]: `GetClaim.Map(claims);`
- `src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs` [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L61-L61]: `ApplyClaimAction.Map(claims);`

## Patterns

Required:
- `ClaimPermissions`
- `ClaimsDbContext`
- `IEndpointRouteBuilder`
- `Map`
- `Map(`
- `MapGroup`
- `RequireAuthorization`
- `Results`
- `TypedResults`
- `TypedResults.`
- `WithSummary`
- `WithSummary(`
Forbidden:
- Avoid `ControllerBase` in this change.
- Avoid `DateTime.Now` in this change.
- Avoid `DateTime.UtcNow` in this change.
- Avoid `IActionResult` in this change.
- Avoid `IClaimRepository` in this change.
- Avoid `[ApiController]` in this change.
- Avoid `[Route(` in this change.

## Workflow

1. Create a dedicated feature file in Features declaring an internal static class with a Map(RouteGroupBuilder group) method. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L4-L4]

## Checklist

- [ ] Verify endpoint is registered in a static Map(RouteGroupBuilder group) method. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L56-L56]
- [ ] Verify endpoint metadata includes .WithName(...) and .WithSummary(...). [src/ClaimFlow.Api/Security/DemoIdentityProvider.cs:L39-L39]
- [ ] Verify authorization policy is set via .RequireAuthorization(ClaimPermissions.WritePolicy) or verified using ClaimPermissions.CanPerform. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L55-L55]
- [ ] Verify read queries use dbContext.Claims.AsNoTracking() directly without a repository wrapper. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L71-L71]
- [ ] Verify optimistic concurrency token is configured on dbContext.Entry(claim).Property(c => c.Version).OriginalValue for state updates. [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L71-L71]
- [ ] Verify timestamps use timeProvider.GetUtcNow() and actor identity uses user.ToActor(). [src/ClaimFlow.Api/Security/DemoIdentityProvider.cs:L56-L56]
- [ ] Are mutating routes secured with .RequireAuthorization(DocumentPermissions.WritePolicy) or relevant policies? [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L55-L55]
- [ ] Does cross-module verification go through IClaimDirectory rather than database foreign keys? [src/Modules/Claims/ClaimFlow.Claims/ClaimsModule.cs:L39-L39]
