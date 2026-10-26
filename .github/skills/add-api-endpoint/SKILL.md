---
name: add-api-endpoint
description: "Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results."
---

> Evidence: value gate verified the facts below against this repository.

Exposes an HTTP endpoint with caller actor extraction and RFC 9457 problem detail mapping for failed domain results.

Applies in: `claim-flow-api`, `claim-flow-building-blocks`, `claim-flow-claims`.

## What is specific to this repository

- The ToActor extension method on ClaimsPrincipal extracts the authenticated caller into a domain Actor using SubjectClaim and NameClaim. [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L14-L14]
- The ToProblem extension method on Result maps business failures into RFC 9457 problem details. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]

## Rules this change touches

- [Invariant] A failed Result instance must contain at least one error in its Errors collection when converted via ToProblem. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L17-L17)
- [Invariant] The user parameter passed to ToActor must not be null; ArgumentNullException is thrown if it is null. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L16-L16)
- [Invariant] A Result converted via ToProblem must not be null and must not represent a successful outcome (result.IsSuccess must be false). Attempting to convert a successful result throws an InvalidOperationException. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L10-L15)
- [BusinessRule] An authenticated ClaimsPrincipal must contain a 'sub' claim; if missing, an InvalidOperationException is thrown with message "Authenticated principal has no 'sub' claim.". (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L16-L18)
- [InformalDecision] Standard OIDC claim names ("sub", "name", and "role") are maintained as raw constant strings rather than mapped inbound claims, maintaining compatibility across OIDC-compliant identity providers. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L8-L11)
- [InformalDecision] `ResultExtensions.ToProblem` provides an extension method on `Result` that standardizes business failure mapping into RFC 9457 Problem Details responses across all HTTP endpoints in the API. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L6-L8)
- [InformalDecision] Standard OIDC claim names ('sub', 'name', 'role') are kept raw without inbound claim mapping (MapInboundClaims = false) to ensure compatibility across compliant identity providers. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L8-L11)

## Context

- The ToActor extension method converts a ClaimsPrincipal into an Actor domain object by extracting the 'sub' claim for the actor ID, and falling back to the actor ID if the 'name' claim is null or absent. [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L14-L19]
- When the first error is not a validation error, ToProblem maps ErrorType.NotFound to 404 Not Found, ErrorType.Forbidden to 403 Forbidden, ErrorType.Unavailable to 503 Service Unavailable, and any other error type to 409 Conflict via TypedResults.Problem, setting the title to the first error's Description and adding the error Code to problem extensions under 'code'. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L26-L36]
- Map registers the POST endpoint at '/{id:guid}/actions' named 'ApplyClaimAction', requiring the ClaimPermissions.WritePolicy authorization policy and declaring responses for 200 OK (ClaimDetailsResponse), validation problem, 403 Forbidden, 404 Not Found, and 409 Conflict. [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L18-L27]

## Start here

`src/ClaimFlow.Api/Security/DemoIdentityProvider.cs` [src/ClaimFlow.Api/Security/DemoIdentityProvider.cs:L34-L73] is the worked example for this change, as it stands in the repository:

```csharp

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
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
```

## The same change elsewhere in this repository

- `src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs` [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]: `public static IResult ToProblem(this Result result)`
- `src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs` [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L18-L18]: `if (first.Type == ErrorType.Validation)`
- `src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs` [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L23-L23]: `return TypedResults.ValidationProblem(errors, title: "One or more business rules were violated.");`
- `src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs` [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L14-L14]: `public static Actor ToActor(this ClaimsPrincipal user)`
- `src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs` [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L17-L17]: `var id = user.FindFirstValue(SubjectClaim)`
- `src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs` [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L20-L20]: `group.MapPost("/{id:guid}/actions", HandleAsync)`
- `src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs` [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L21-L21]: `.RequireAuthorization(ClaimPermissions.WritePolicy)`

## Patterns

Required:
- `ClaimsDbContext`
- `IEndpointRouteBuilder`
- `Map(`
- `Map(RouteGroupBuilder group)`
- `RouteGroupBuilder`
- `TimeProvider`
- `ToActor`
- `ToProblem`
- `ToProblem()`
- `TypedResults`
- `WithName`
- `WithSummary`
Forbidden:
- Avoid `ControllerBase` in this change.
- Avoid `DateTime.Now` in this change.
- Avoid `DateTime.UtcNow` in this change.
- Avoid `IActionResult` in this change.
- Avoid `IRepository` in this change.
- Avoid `[ApiController]` in this change.
- Avoid `[HttpGet]` in this change.
- Avoid `[HttpPost]` in this change.

## Workflow

1. Define endpoint route and secure it with RequireAuthorization. [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L13-L13]
2. Extract Actor from ClaimsPrincipal using user.ToActor(). [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L14-L14]
3. If result failed, map to an RFC 9457 response using result.ToProblem(). [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L67-L67]

## Checklist

- [ ] Endpoint requires authorization so ClaimsPrincipal has a valid 'sub' claim [src/ClaimFlow.BuildingBlocks/Security/ClaimsPrincipalExtensions.cs:L14-L14]
- [ ] User actor is extracted via user.ToActor() [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L54-L54]
- [ ] Failed Result instances are converted to RFC 9457 responses using result.ToProblem() [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L67-L67]
- [ ] The endpoint class is internal static under the Features namespace. [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L15-L15]
- [ ] The class declares public static void Map(RouteGroupBuilder group). [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L19-L19]
- [ ] The route mapping calls WithName and WithSummary. [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L22-L22]
- [ ] Endpoint produces declarations specify success responses and problem codes via Produces and ProducesProblem. [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L26-L26]
- [ ] Authorization policies from ClaimPermissions are configured when restricting access. [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L21-L21]
- [ ] Database queries on ClaimsDbContext for reads apply AsNoTracking(). [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L35-L35]
- [ ] Domain error results are mapped to HTTP responses using .ToProblem(). [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]
- [ ] Asynchronous database operations receive and forward the CancellationToken. [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L37-L37]
- [ ] Are .WithName(...) and .WithSummary(...) configured for OpenAPI discovery? [src/Modules/Claims/ClaimFlow.Claims/Features/ApplyClaimAction.cs:L22-L22]
