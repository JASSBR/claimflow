using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Claims.Features;

internal static class GetClaimStats
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/stats", HandleAsync)
            .WithName("GetClaimStats")
            .WithSummary("Portfolio dashboard: claim count per status and amounts")
            .Produces<ClaimStatsResponse>();

    private static async Task<IResult> HandleAsync(ClaimsDbContext dbContext, CancellationToken cancellationToken)
    {
        // One GROUP BY (status, type) round trip feeds every figure; missing keys are filled in memory
        // so the client always receives every status and type.
        var rows = await dbContext.Claims
            .GroupBy(claim => new { claim.Status, claim.Type })
            .Select(group => new
            {
                group.Key.Status,
                group.Key.Type,
                Count = group.Count(),
                Claimed = group.Sum(claim => claim.ClaimedAmount),
                Approved = group.Sum(claim => claim.ApprovedAmount ?? 0),
            })
            .ToListAsync(cancellationToken);

        var countByStatus = Enum.GetValues<ClaimStatus>()
            .ToDictionary(status => status, status => rows.Where(row => row.Status == status).Sum(row => row.Count));
        var byType = Enum.GetValues<ClaimType>()
            .ToDictionary(
                type => type,
                type => new ClaimTypeStats(
                    rows.Where(row => row.Type == type).Sum(row => row.Count),
                    rows.Where(row => row.Type == type).Sum(row => row.Claimed)));

        return TypedResults.Ok(new ClaimStatsResponse(
            countByStatus,
            byType,
            rows.Sum(row => row.Claimed),
            rows.Sum(row => row.Approved)));
    }
}
