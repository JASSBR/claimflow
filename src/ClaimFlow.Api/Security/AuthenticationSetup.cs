using System.Text;
using ClaimFlow.BuildingBlocks.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ClaimFlow.Api.Security;

internal static class AuthenticationSetup
{
    private const string HubsPath = "/hubs";

    /// <summary>
    /// One validation pipeline for both modes: only the key source differs (provider JWKS vs. demo HMAC key).
    /// Authorization code never knows which provider issued the token.
    /// </summary>
    public static WebApplicationBuilder AddClaimFlowAuthentication(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(AuthOptions.SectionName);
        builder.Services.AddOptions<AuthOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart();
        var options = section.Get<AuthOptions>() ?? new AuthOptions();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                // Keep standard OIDC claim names ("sub", "name", "role") instead of legacy WS-* URIs.
                jwt.MapInboundClaims = false;
                jwt.Audience = options.Audience;
                jwt.TokenValidationParameters.NameClaimType = ClaimsPrincipalExtensions.NameClaim;
                jwt.TokenValidationParameters.RoleClaimType = ClaimsPrincipalExtensions.RoleClaim;

                if (options.Mode == AuthMode.Demo && options.DemoSigningKey is not null)
                {
                    jwt.TokenValidationParameters.ValidIssuer = AuthOptions.DemoIssuer;
                    jwt.TokenValidationParameters.IssuerSigningKey = DemoSigningKey(options.DemoSigningKey);
                }
                else
                {
                    jwt.Authority = options.Authority;
                }

                // Browsers cannot set headers on WebSocket requests: SignalR sends the token as a query parameter.
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments(HubsPath, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        builder.Services.AddAuthorization();
        return builder;
    }

    public static SymmetricSecurityKey DemoSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
