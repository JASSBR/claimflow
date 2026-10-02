using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Claims.Persistence;

internal sealed class ClaimDirectory(ClaimsDbContext dbContext) : IClaimDirectory
{
    public Task<ClaimSnapshot?> FindAsync(Guid claimId, CancellationToken cancellationToken) =>
        Snapshots(dbContext.Claims.Where(claim => claim.Id == new ClaimId(claimId))).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ClaimSnapshot>> FindByPolicyAsync(string policyNumber, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(policyNumber);

        // Policy numbers are stored upper-case: normalise the input once, keep an index-friendly equality in SQL.
        var normalized = policyNumber.Trim().ToUpperInvariant();
        // Order before projecting: EF cannot translate a sort on a constructed record.
        return await Snapshots(dbContext.Claims.Where(claim => claim.PolicyNumber == normalized).OrderByDescending(claim => claim.DeclaredAt))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<ClaimSnapshot> Snapshots(IQueryable<Claim> claims) =>
        claims.AsNoTracking()
            .Select(claim => new ClaimSnapshot(
                claim.Id.Value,
                claim.Number,
                claim.PolicyNumber,
                claim.Type.ToString(),
                claim.Status.ToString(),
                claim.IncidentDate,
                claim.DeclaredAt,
                claim.Description,
                claim.ClaimedAmount,
                claim.ApprovedAmount));
}
