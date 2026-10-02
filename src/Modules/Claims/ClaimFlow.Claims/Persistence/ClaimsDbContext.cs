using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Claims.Persistence;

/// <summary>
/// The module's unit of work. Features use it directly instead of a repository layer:
/// DbContext already is a repository + unit of work, and wrapping it would only hide EF's query power (see ADR 0004).
/// </summary>
internal sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public const string Schema = "claims";
    private const string ClaimNumberSequence = "claim_number_seq";

    public DbSet<Claim> Claims => Set<Claim>();

    /// <summary>
    /// Human-readable, collision-free claim reference (SIN-2026-000123). A sequence never hands out the same value twice,
    /// even across concurrent transactions; a rolled-back declaration just leaves a gap, which is acceptable for a reference.
    /// </summary>
    public async Task<string> NextClaimNumberAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var value = await Database
            .SqlQuery<long>($"SELECT nextval('claims.claim_number_seq') AS \"Value\"")
            .SingleAsync(cancellationToken);
        return $"SIN-{now:yyyy}-{value:D6}";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasSequence<long>(ClaimNumberSequence);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
        modelBuilder.ApplyOutbox();
    }
}
