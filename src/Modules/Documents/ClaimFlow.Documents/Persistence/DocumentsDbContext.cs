using ClaimFlow.Documents.Analysis;
using ClaimFlow.Documents.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Documents.Persistence;

internal sealed class DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : DbContext(options)
{
    public const string Schema = "documents";

    public DbSet<ClaimDocument> Documents => Set<ClaimDocument>();

    public DbSet<ClaimAnalysis> Analyses => Set<ClaimAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ClaimDocument>(document =>
        {
            document.ToTable("claim_documents");
            document.HasKey(d => d.Id);
            document.Property(d => d.Id).HasConversion(id => id.Value, value => new DocumentId(value)).ValueGeneratedNever();
            // A claim id, not a foreign key: the claims table belongs to another module's schema (ADR 0001).
            document.HasIndex(d => new { d.ClaimId, d.UploadedAt });
            document.Property(d => d.FileName).HasMaxLength(ClaimDocument.FileNameMaxLength);
            document.Property(d => d.ContentType).HasMaxLength(64);
            document.Property(d => d.Sha256).HasMaxLength(64).IsFixedLength();
            document.Property(d => d.UploadedById).HasMaxLength(128);
            document.Property(d => d.UploadedByName).HasMaxLength(200);
            document.Ignore(d => d.StorageKey);
            document.Ignore(d => d.DomainEvents);
        });

        modelBuilder.Entity<ClaimAnalysis>(analysis =>
        {
            analysis.ToTable("claim_analyses");
            analysis.HasKey(a => a.Id);
            analysis.HasIndex(a => new { a.ClaimId, a.CreatedAt });
            analysis.Property(a => a.RequestedById).HasMaxLength(128);
            analysis.Property(a => a.RequestedByName).HasMaxLength(200);
            analysis.Property(a => a.Model).HasMaxLength(64);
            // Citations travel with their analysis and are never queried alone: one jsonb column, no join table.
            analysis.OwnsMany(a => a.Citations, citations => citations.ToJson());
        });
    }
}
