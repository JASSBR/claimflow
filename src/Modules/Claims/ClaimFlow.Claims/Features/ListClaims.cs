using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Claims.Features;

internal sealed record ListClaimsQuery(ClaimStatus? Status, string? Search, int? Page, int? PageSize);

internal static class ListClaims
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    // Keeps (page - 1) * pageSize far from int overflow, which would become a negative OFFSET and a 500.
    private const int MaxPage = 100_000;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/", HandleAsync)
            .WithName("ListClaims")
            .WithSummary("List claims, newest first, filtered by status and/or claim or policy number")
            .Produces<PagedResponse<ClaimSummaryResponse>>();

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListClaimsQuery query,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var page = Math.Clamp(query.Page ?? 1, 1, MaxPage);
        var pageSize = Math.Clamp(query.PageSize ?? DefaultPageSize, 1, MaxPageSize);

        var claims = dbContext.Claims.AsNoTracking();
        if (query.Status is { } status)
        {
            claims = claims.Where(claim => claim.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Both columns are stored upper-case, so an upper-cased Contains is enough (no ILIKE). It translates to strpos(),
            // a sequential scan: fine at back-office volumes; a pg_trgm GIN index is the upgrade path.
            var search = query.Search.Trim().ToUpperInvariant();
            claims = claims.Where(claim => claim.Number.Contains(search) || claim.PolicyNumber.Contains(search));
        }

        var totalCount = await claims.CountAsync(cancellationToken);
        var items = await claims
            .OrderByDescending(claim => claim.DeclaredAt)
            // Tie-breaker: without it, rows sharing a timestamp may appear on two pages or on none.
            .ThenByDescending(claim => claim.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(claim => new ClaimSummaryResponse(
                claim.Id.Value,
                claim.Number,
                claim.PolicyNumber,
                claim.Type,
                claim.Status,
                claim.ClaimedAmount,
                claim.ApprovedAmount,
                claim.IncidentDate,
                claim.DeclaredAt,
                claim.LastUpdatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(new PagedResponse<ClaimSummaryResponse>(items, page, pageSize, totalCount));
    }
}
