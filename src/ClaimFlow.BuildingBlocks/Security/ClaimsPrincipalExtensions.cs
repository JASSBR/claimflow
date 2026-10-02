using System.Security.Claims;
using ClaimFlow.SharedKernel;

namespace ClaimFlow.BuildingBlocks.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Standard OIDC claim names, kept raw (MapInboundClaims = false) so any compliant provider works unchanged.</summary>
    public const string SubjectClaim = "sub";
    public const string NameClaim = "name";
    public const string RoleClaim = "role";

    /// <summary>The authenticated caller as a domain actor. Endpoints calling this are behind RequireAuthorization.</summary>
    public static Actor ToActor(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var id = user.FindFirstValue(SubjectClaim)
            ?? throw new InvalidOperationException("Authenticated principal has no 'sub' claim.");
        return new Actor(id, user.FindFirstValue(NameClaim) ?? id);
    }
}
