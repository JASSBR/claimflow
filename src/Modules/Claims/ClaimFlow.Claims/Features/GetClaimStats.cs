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
        // One GROUP BY round trip; statuses with no claim are filled in memory so the client always gets every key.
        var rows = await dbContext.Claims
            .GroupBy(claim => claim.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count(),
                Claimed = group.Sum(claim => claim.ClaimedAmount),
                Approved = group.Sum(claim => claim.ApprovedAmount ?? 0),
            })
            .ToListAsync(cancellationToken);

        var countByStatus = Enum.GetValues<ClaimStatus>()
            .ToDictionary(status => status, status => rows.Find(row => row.Status == status)?.Count ?? 0);

        return TypedResults.Ok(new ClaimStatsResponse(
            countByStatus,
            rows.Sum(row => row.Claimed),
            rows.Sum(row => row.Approved)));
    }
}
