using ClaimFlow.Claims.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimFlow.Claims.Persistence;

internal sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    // Identity-provider subject ids are opaque strings (GUIDs for Entra ID, UUIDs for Keycloak): 128 covers both.
    private const int ActorIdMaxLength = 128;
    private const int ActorNameMaxLength = 200;

    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claims");
        builder.HasKey(claim => claim.Id);
        builder.Property(claim => claim.Id)
            .HasConversion(id => id.Value, value => new ClaimId(value))
            .ValueGeneratedNever();

        builder.Property(claim => claim.Number).HasMaxLength(20);
        builder.HasIndex(claim => claim.Number).IsUnique();
        builder.Property(claim => claim.PolicyNumber).HasMaxLength(10);
        builder.HasIndex(claim => claim.PolicyNumber);
        builder.Property(claim => claim.Description).HasMaxLength(Claim.DescriptionMaxLength);
        builder.Property(claim => claim.DeclaredById).HasMaxLength(ActorIdMaxLength);
        builder.Property(claim => claim.DeclaredByName).HasMaxLength(ActorNameMaxLength);
        builder.Property(claim => claim.ApprovedById).HasMaxLength(ActorIdMaxLength);
        builder.Property(claim => claim.ClaimedAmount).HasPrecision(12, 2);
        builder.Property(claim => claim.ApprovedAmount).HasPrecision(12, 2);

        // Enums stored as text: readable in SQL and safe against enum member reordering.
        builder.Property(claim => claim.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(claim => claim.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(claim => new { claim.Status, claim.DeclaredAt });

        // Npgsql maps a uint row version onto PostgreSQL's xmin: optimistic concurrency without an extra column.
        builder.Property(claim => claim.Version).IsRowVersion();

        builder.OwnsMany(claim => claim.History, history =>
        {
            history.ToTable("claim_history");
            history.WithOwner().HasForeignKey("ClaimId");
            history.HasKey(change => change.Id);
            // Client-generated key: tells EF a new history line is an INSERT, not an UPDATE of an existing row.
            history.Property(change => change.Id).ValueGeneratedNever();
            history.Property(change => change.From).HasConversion<string>().HasMaxLength(32);
            history.Property(change => change.To).HasConversion<string>().HasMaxLength(32);
            history.Property(change => change.Action).HasConversion<string>().HasMaxLength(32);
            history.Property(change => change.Reason).HasMaxLength(1000);
            history.Property(change => change.ActorId).HasMaxLength(ActorIdMaxLength);
            history.Property(change => change.ActorName).HasMaxLength(ActorNameMaxLength);
        });
        builder.Navigation(claim => claim.History).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(claim => claim.AllowedActions);
        builder.Ignore(claim => claim.DomainEvents);
    }
}
