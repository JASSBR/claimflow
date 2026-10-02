using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClaimFlow.Documents.Persistence;

/// <summary>Used only by `dotnet ef` to generate migrations; never at runtime.</summary>
internal sealed class DocumentsDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseNpgsql("Host=localhost;Database=claimflow", DocumentsModule.ConfigureNpgsql)
            .Options);
}
