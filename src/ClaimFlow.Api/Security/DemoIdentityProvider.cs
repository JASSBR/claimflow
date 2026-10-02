using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClaimFlow.Api.Security;

public sealed record DemoPersona(string Id, string Name, string Title, string Summary, IReadOnlyList<string> Roles);

public sealed record DemoTokenRequest(string PersonaId);

public sealed record DemoTokenResponse(string AccessToken, DateTimeOffset ExpiresAt, DemoPersona User);

/// <summary>
/// A deliberately tiny identity provider for the public demo: pick a persona, get a signed JWT.
/// It is only mapped when Auth:Mode is Demo; in Oidc mode the same API trusts a real provider instead (ADR 0008).
/// </summary>
internal static class DemoIdentityProvider
{
    public static readonly IReadOnlyList<DemoPersona> Personas =
    [
        new("lea", "Léa Martin", "Gestionnaire sinistres",
            "Instruit les dossiers et accepte jusqu'à sa délégation de 10 000 €.", [ClaimsRoles.Handler]),
        new("karim", "Karim Benali", "Responsable indemnisation",
            "Accepte sans plafond et libère les paiements — jamais ceux qu'il a lui-même acceptés.", [ClaimsRoles.Manager]),
        new("nadia", "Nadia Haddad", "Responsable indemnisation",
            "Seconde signature : libère les paiements acceptés par un collègue.", [ClaimsRoles.Manager]),
        new("sophie", "Sophie Laurent", "Auditrice interne",
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
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                AuthenticationSetup.DemoSigningKey(options.Value.DemoSigningKey!), SecurityAlgorithms.HmacSha256),
        });

        return TypedResults.Ok(new DemoTokenResponse(token, expiresAt, persona));
    }
}
