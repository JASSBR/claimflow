using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClaimFlow.Claims.Persistence;

/// <summary>Used only by `dotnet ef` to generate migrations; never at runtime.</summary>
internal sealed class ClaimsDbContextFactory : IDesignTimeDbContextFactory<ClaimsDbContext>
{
    public ClaimsDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseNpgsql("Host=localhost;Database=claimflow", ClaimsModule.ConfigureNpgsql)
            .Options);
}
