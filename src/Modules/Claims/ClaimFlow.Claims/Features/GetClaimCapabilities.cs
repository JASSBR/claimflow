using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ClaimFlow.Claims.Features;

internal static class GetClaimCapabilities
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/capabilities", Handle)
            .WithName("GetClaimCapabilities")
            .WithSummary("What the current user may do: declare claims, approval limit")
            .Produces<ClaimCapabilitiesResponse>();

    private static Ok<ClaimCapabilitiesResponse> Handle(ClaimsPrincipal user, ClaimPermissions permissions) =>
        TypedResults.Ok(new ClaimCapabilitiesResponse(
            ClaimPermissions.CanDeclare(user),
            permissions.ApprovalLimitOf(user),
            [.. user.FindAll(ClaimsPrincipalExtensions.RoleClaim).Select(role => role.Value)]));
}
