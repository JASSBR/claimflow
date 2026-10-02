using System.ComponentModel.DataAnnotations;

namespace ClaimFlow.Api.Security;

public enum AuthMode
{
    /// <summary>Tokens from a real OpenID Connect provider (Entra ID, Keycloak…), validated against its JWKS.</summary>
    Oidc,

    /// <summary>Tokens minted by the built-in demo identity provider, so reviewers can log in as a persona in one click.</summary>
    Demo,
}

internal sealed class AuthOptions : IValidatableObject
{
    public const string SectionName = "Auth";
    public const string DemoIssuer = "claimflow-demo";
    public const string ApiAudience = "claimflow-api";

    public AuthMode Mode { get; init; } = AuthMode.Oidc;

    /// <summary>OIDC issuer URL, e.g. https://login.microsoftonline.com/{tenant}/v2.0. Required in Oidc mode.</summary>
    public string? Authority { get; init; }

    public string Audience { get; init; } = ApiAudience;

    /// <summary>HMAC key of the demo provider (≥ 32 bytes). Provided by a secret store, never committed.</summary>
    public string? DemoSigningKey { get; init; }

    public TimeSpan DemoTokenLifetime { get; init; } = TimeSpan.FromHours(8);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Mode == AuthMode.Oidc && string.IsNullOrWhiteSpace(Authority))
        {
            yield return new ValidationResult("Auth:Authority is required when Auth:Mode is Oidc.", [nameof(Authority)]);
        }

        if (Mode == AuthMode.Demo && (DemoSigningKey is null || DemoSigningKey.Length < 32))
        {
            yield return new ValidationResult("Auth:DemoSigningKey (≥ 32 chars) is required when Auth:Mode is Demo.", [nameof(DemoSigningKey)]);
        }
    }
}
